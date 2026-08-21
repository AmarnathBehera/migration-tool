import React, { useState } from 'react';
import {
    BarItemDraggable,
    BarItemGroup,
    Button,
    ButtonColor,
    ButtonSize,
    Card,
    Cols,
    Column,
    Row,
    Spacing,
    Stack,
    TextArea
} from '@kentico/xperience-admin-components';

import {
    usePageCommand,
    usePageCommandProvider
} from '@kentico/xperience-admin-base';

/** Copy of C# DatabaseTable class */
interface DatabaseTable {
    name: string;
    columns: string[];
}

/** Copy of C# SavedQuery class */
interface SavedQuery {
    id: number;
    name: string;
    text: string;
    order: number;
}

/** Copy of C# EditSqlTemplateClientProperties class */
interface EditQueryClientProperties {
    tables: DatabaseTable[];
    query: string | undefined;
    savedQueries: SavedQuery[];
    reportingChannelSettingId: number;
}

/** Copy of C# SqlBrowserQueryResult class */
interface SqlBrowserQueryResult {
    columns: string[];
    rows: string[][];
    errorMessage: string | undefined;
    // autoSavedQuery: SavedQuery | undefined;
}

export const EditQuery = (props: EditQueryClientProperties) => {
    const { executeCommand } = usePageCommandProvider();

    const textAreaRef = React.createRef<HTMLTextAreaElement>();

    const [queryText, setQueryText] = useState(props.query);
    const [savedQueries, setSavedQueries] = useState(props.savedQueries);
    const [queryResult, setQueryResult] = useState<SqlBrowserQueryResult>();
    const [isRunningQuery, setIsRunningQuery] = useState(false);

    const { execute: runSql } =
        usePageCommand<SqlBrowserQueryResult, string>('RunSql', {
            after: result => {
                setIsRunningQuery(false);

                if (!result) {
                    return;
                }

                setQueryResult(result);

            }
        });

    const { execute: notify } =
        usePageCommand<void, string>('Notify');
    interface ExportConfirmationDialogModel {
        exportType: string;
        fileName: string;
    }

    interface ExportResult {
        base64: string;
        fileName: string;
        contentType: string;
    }

    const exportClick = async () => {
        if (!queryResult || queryResult.rows.length === 0) {
            return;
        }

        const model: ExportConfirmationDialogModel = {
            exportType: 'csv',
            fileName: 'export'
        };

        try {
            const raw = await executeCommand<ExportResult | null, ExportConfirmationDialogModel>('ExportQuery', model);

            console.log('exportQuery raw result:', raw);

            if (raw == null) {
                notify('Export failed: no result returned from server.');
                return;
            }

            // Normalize wrapper layers commonly used by server frameworks or the admin SDK.
            let payload: any = raw as any;
            // Common wrapper property names
            const unwrapKeys = ['value', 'Value', 'result', 'Result', 'data', 'Data'];
            for (const k of unwrapKeys) {
                if (payload && typeof payload === 'object' && k in payload) {
                    payload = payload[k];
                }
            }

            // If the payload is a primitive string, treat it as base64 content (legacy behavior)
            if (typeof payload === 'string') {
                const legacyBase64 = payload;
                const legacyFileName = model.fileName ? model.fileName : `export-${Date.now()}.dat`;
                const legacyContentType = 'application/octet-stream';

                const legacyHref = `data:${legacyContentType};base64,${legacyBase64}`;
                const legacyLink = document.createElement('a');
                legacyLink.style.display = 'none';
                legacyLink.href = legacyHref;
                legacyLink.download = legacyFileName;
                document.body.appendChild(legacyLink);
                legacyLink.click();
                document.body.removeChild(legacyLink);

                notify(`Export downloaded: ${legacyFileName}`);
                return;
            }

            // If payload is an object, try known property names (support PascalCase from C#)
            const base64 = payload?.base64 ?? payload?.Base64 ?? payload?.Base64String ?? payload?.content ?? payload?.Content ?? '';
            const contentType = payload?.contentType ?? payload?.ContentType ?? payload?.content_type ?? 'application/octet-stream';
            const fileName = payload?.fileName ?? payload?.FileName ?? payload?.filename ?? model.fileName ?? `export-${Date.now()}.dat`;

            if (!base64 || typeof base64 !== 'string' || base64.length === 0) {
                console.warn('Export payload did not contain base64 content', payload);
                notify('Export failed: empty content returned from server.');
                return;
            }

            const href = `data:${contentType};base64,${base64}`;
            const link = document.createElement('a');
            link.style.display = 'none';
            link.href = href;
            link.download = fileName;

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            notify(`Export downloaded: ${fileName}`);
        }
        catch (ex) {
            console.error('Export download failed', ex);
            notify('Export failed: an error occurred. Check console for details.');
        }
    };
    const { execute: saveQuery } =
        usePageCommand<SavedQuery, SavedQuery>('SaveQuery', {
            after: newQuery => {
                if (!newQuery) {
                    return;
                }

                const newQueries = [...savedQueries];
                newQueries.push(newQuery);
                setSavedQueries(newQueries);
            }
        });

    const { execute: renameQuery } =
        usePageCommand<SavedQuery, SavedQuery>('RenameQuery', {
            after: updatedQuery => {
                if (!updatedQuery) {
                    return;
                }

                setSavedQueries(currentQueries =>
                    currentQueries.map(query => {
                        if (query.id !== updatedQuery.id) {
                            return query;
                        }

                        return updatedQuery;
                    })
                );
            }
        });

    const { execute: deleteQuery } =
        usePageCommand<number, number>('DeleteQuery', {
            after: async id => {
                if (!id || id <= 0) {
                    return;
                }

                const newQueries =
                    savedQueries.filter(q => q.id !== id);

                newQueries.forEach((query, i) => {
                    query.order = i;
                });

                setSavedQueries(newQueries);

                await executeCommand<boolean, SavedQuery[]>(
                    'UpdateSavedOrder',
                    newQueries
                );
            }
        });

    const renameClick = (query: SavedQuery) => {
        const name = prompt('Enter query name', query.name);

        if (!name) {
            return;
        }

        renameQuery({
            ...query,
            name
        });
    };

    const deleteClick = (id: number) => {
        const confirmed =
            confirm('Are you sure you want to delete this query?');

        if (!confirmed) {
            return;
        }

        deleteQuery(id);
    };

    const transferClick = (text: string) => {
        if (!textAreaRef.current) {
            return;
        }

        setQueryText(text);
        textAreaRef.current.textContent = text;

        notify('Query copied to editor');
    };

    const clearClick = () => {
        if (!textAreaRef.current) {
            return;
        }

        setQueryText('');
        textAreaRef.current.textContent = '';
    };

    const runClick = () => {
        if (!queryText || queryText.length <= 0) {
            alert('Please enter a query to execute.');
            return;
        }

        setIsRunningQuery(true);
        runSql(queryText);
    };

    const copyClick = async () => {
        if (!queryText || queryText.length <= 0) {
            return;
        }

        await navigator.clipboard.writeText(queryText);

        notify('Query copied to clipboard');
    };

    const saveClick = async () => {
        if (!queryText || queryText.length <= 0) {
            alert('Please enter a query in the editor');
            return;
        }

        const name = prompt('Enter new query name');

        if (!name) {
            return;
        }

        saveQuery({
            id: 0,
            order: 0,
            name,
            text: queryText
        });
    };

    const savedQueryDragEnd = async (dropResult: any) => {
        if (
            dropResult.source.index ===
            dropResult.destination?.index
        ) {
            return;
        }

        const sourceIndex = dropResult.source.index;
        const destinationIndex =
            dropResult.destination?.index || 0;

        const newQueries = updateQueryOrder(
            savedQueries,
            sourceIndex,
            destinationIndex
        );

        const updateSuccessful =
            await executeCommand<boolean, SavedQuery[]>(
                'UpdateSavedOrder',
                newQueries
            );

        if (!updateSuccessful) {
            updateQueryOrder(
                newQueries,
                sourceIndex,
                destinationIndex
            );
        }
    };

    const updateQueryOrder = (
        oldQueries: SavedQuery[],
        sourceIndex: number,
        destinationIndex: number
    ) => {
        const newQueries = [...oldQueries];

        const [sourceField] =
            newQueries.splice(sourceIndex, 1);

        newQueries.splice(destinationIndex, 0, sourceField);

        newQueries.forEach((query, i) => {
            query.order = i;
        });

        setSavedQueries(newQueries);

        return newQueries;
    };

    const renderSavedQueries = () =>
        savedQueries.map(q => (
            <BarItemDraggable
                key={q.id}
                index={q.order}
                draggableId={q.id.toString()}
                leadingButtons={[
                    {
                        label: 'Run',
                        icon: 'xp-caret-right',
                        tooltip: 'Execute query',
                        onClick: () => {
                            setQueryText(q.text);
                            setIsRunningQuery(true);
                            runSql(q.text);
                        }
                    },
                    {
                        label: 'Copy',
                        icon: 'xp-doc-copy',
                        tooltip: 'Copy query to editor',
                        onClick: () => transferClick(q.text)
                    },
                    {
                        label: 'Rename',
                        icon: 'xp-edit',
                        tooltip: 'Rename query',
                        onClick: () => renameClick(q)
                    },
                    {
                        label: 'Delete',
                        icon: 'xp-bin',
                        tooltip: 'Delete query',
                        onClick: () => deleteClick(q.id)
                    }
                ]}
                headerColumns={[
                    {
                        content: <span>{q.name}</span>
                    }
                ]}
            >
                <span>{q.text}</span>
            </BarItemDraggable>
        ));

    const renderQueryResult = () => {
        if (isRunningQuery) {
            return (
                <Card headline="Results">
                    <span>Running query...</span>
                </Card>
            );
        }

        if (!queryResult) {
            return null;
        }

        if (queryResult.errorMessage) {
            return (
                <Card headline="Results">
                    <span>{queryResult.errorMessage}</span>
                </Card>
            );
        }

        if (queryResult.rows.length === 0) {
            return (
                <Card headline="Results">
                    <span>No rows returned.</span>
                </Card>
            );
        }

        // inside the existing renderQueryResult() function, replace or augment the Card header area
        return (
            <Card headline="Results">
                {/* top bar: left can be empty or show summary, right shows Export button */}
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
                    <div>
                        {/* optional: show row count */}
                        <strong>{queryResult.rows.length} rows</strong>
                    </div>

                    <div>
                        <Button
                            label="Export CSV"
                            color={ButtonColor.Secondary}
                            size={ButtonSize.S}
                            onClick={() => exportClick()}

                        />
                    </div>
                </div>

                <div style={{ overflowX: 'auto' }}>
                    <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                        <thead>
                            <tr>
                                {queryResult.columns.map(column => (
                                    <th key={column} style={{ textAlign: 'left', borderBottom: '1px solid #d6d9dc', padding: '0.5rem' }}>
                                        {column}
                                    </th>
                                ))}
                            </tr>
                        </thead>

                        <tbody>
                            {queryResult.rows.map((row, rowIndex) => (
                                <tr key={rowIndex}>
                                    {row.map((value, columnIndex) => (
                                        <td key={`${rowIndex}-${columnIndex}`} style={{ borderBottom: '1px solid #eef0f2', padding: '0.5rem', verticalAlign: 'top' }}>
                                            {value}
                                        </td>
                                    ))}
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </Card>
        );
    };


    const renderTextActions = () => (
        <>
            <Button
                label="Save"
                color={ButtonColor.Primary}
                size={ButtonSize.S}
                onClick={saveClick}
                icon="xp-doc-plus"
            />

            <Button
                label="Copy"
                color={ButtonColor.Tertiary}
                size={ButtonSize.S}
                onClick={copyClick}
                icon="xp-doc-copy"
            />

            <Button
                label="Clear"
                color={ButtonColor.Tertiary}
                size={ButtonSize.S}
                onClick={clearClick}
                icon="xp-doc-torn"
            />

            <Button
                label="Export"
                color={ButtonColor.Secondary}
                size={ButtonSize.S}
                onClick={exportClick}
                icon="xp-arrows-v"
            />

            { }
        </>
    );

    return (
        <Row spacing={Spacing.XL}>
            <Column cols={Cols.Col1} />

            <Column cols={Cols.Col10}>
                <Stack spacing={Spacing.XL}>
                    <Button
                        label="Run"
                        icon="xp-caret-right"
                        color={ButtonColor.Primary}
                        onClick={runClick}
                    />

                    <Card headline="Query">
                        <TextArea
                            minRows={10}
                            maxRows={40}
                            value={queryText}
                            textAreaRef={textAreaRef}
                            placeholder="Enter SQL query..."
                            onChange={(e) =>
                                setQueryText(e.target.value)
                            }
                            renderActions={renderTextActions}
                        />
                    </Card>

                    {renderQueryResult()}

                    {savedQueries.length > 0 && (
                        <Card headline="Saved queries">
                            <BarItemGroup
                                droppableId="savedQueryDroppable"
                                onDragEnd={savedQueryDragEnd}
                            >
                                {renderSavedQueries()}
                            </BarItemGroup>
                        </Card>
                    )}
                </Stack>
            </Column>

            <Column cols={Cols.Col1} />
        </Row>
    );
};
export const EditQueryTemplate = EditQuery;

export default EditQuery;
