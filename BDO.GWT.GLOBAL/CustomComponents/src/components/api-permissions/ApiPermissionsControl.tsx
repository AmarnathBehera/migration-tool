import React, { useState, useEffect, useCallback } from "react";
import { useFormComponentCommandProvider, FormComponentProps } from "@kentico/xperience-admin-base";

interface ChannelOption { id: number; displayName: string; }
interface UserOption    { id: number; displayName: string; }
interface MethodGroupOption { id: number; displayName: string; description: string; }

interface MatrixRow {
    userId: number;
    methodGroupId: number;
    userDisplayName: string;
    methodGroupName: string;
    allowRead: boolean;
    allowManage: boolean;
}

interface InitResult {
    channels: ChannelOption[];
    users: UserOption[];
    methodGroups: MethodGroupOption[];
}

interface LoadMatrixResult { rows: MatrixRow[]; }
interface ToggleResult { success: boolean; message: string; }
type FilterType = "users" | "methodgroup";

export const ApiPermissionsControl = (props: FormComponentProps) => {
    const [channels, setChannels]         = useState<ChannelOption[]>([]);
    const [users, setUsers]               = useState<UserOption[]>([]);
    const [methodGroups, setMethodGroups] = useState<MethodGroupOption[]>([]);
    const [matrix, setMatrix]             = useState<MatrixRow[]>([]);

    const [channelId, setChannelId]   = useState<number>(0);
    const [filterType, setFilterType] = useState<FilterType>("users");
    const [selectedId, setSelectedId] = useState<number>(0);
    const [searchText, setSearchText] = useState<string>("");
    const [appliedSearch, setAppliedSearch] = useState<string>("");
    const [loading, setLoading]       = useState(false);
    const [initialized, setInitialized] = useState(false);

    const { executeCommand } = useFormComponentCommandProvider();

    useEffect(() => {
        (async () => {
            const result = await executeCommand<InitResult, string>(props, "Initialize", "");
            if (!result) return;
            setChannels(result.channels ?? []);
            setUsers(result.users ?? []);
            setMethodGroups(result.methodGroups ?? []);
            setChannelId(result.channels?.[0]?.id ?? 0);
            setSelectedId(result.users?.[0]?.id ?? 0);
            setInitialized(true);
        })();
    }, []);

    const loadMatrix = useCallback(async (ft: FilterType, sid: number, chId: number) => {
        if (sid <= 0) return;
        setLoading(true);
        const result = await executeCommand<LoadMatrixResult, string>(
            props, "LoadMatrix",
            JSON.stringify({ filterType: ft, selectedId: sid, channelId: chId })
        );
        setMatrix(result?.rows ?? []);
        setLoading(false);
    }, [executeCommand, props]);

    useEffect(() => {
        if (initialized) loadMatrix(filterType, selectedId, channelId);
    }, [filterType, selectedId, channelId, initialized]);

    const handleFilterTypeChange = (newFilter: FilterType) => {
        setFilterType(newFilter);
        setAppliedSearch("");
        setSearchText("");
        const firstId = newFilter === "users"
            ? (users[0]?.id ?? 0)
            : (methodGroups[0]?.id ?? 0);
        setSelectedId(firstId);
    };

    const handleToggle = async (
        userId: number, methodGroupId: number,
        permType: "read" | "manage", allow: boolean
    ) => {
        setMatrix(prev => prev.map(row =>
            row.userId === userId && row.methodGroupId === methodGroupId
                ? { ...row, [permType === "read" ? "allowRead" : "allowManage"]: allow }
                : row
        ));
        await executeCommand<ToggleResult, string>(
            props, "TogglePermission",
            JSON.stringify({ userId, methodGroupId, channelId, permissionType: permType, allow })
        );
    };

    const options = filterType === "users" ? users : methodGroups;

    const filteredMatrix = appliedSearch.trim()
        ? matrix.filter(row => {
            const name = filterType === "users" ? row.methodGroupName : row.userDisplayName;
            return name.toLowerCase().includes(appliedSearch.toLowerCase());
        })
        : matrix;

    return (
        <div style={{ padding: "16px", fontFamily: "inherit" }}>
            {/* ── Filter bar ── */}
            <table style={filterTableStyle}>
                <tbody>
                    <tr>
                        <td style={filterLabelStyle}>Channel:</td>
                        <td>
                            <select style={filterSelectStyle} value={channelId}
                                onChange={e => setChannelId(Number(e.target.value))}>
                                {channels.map(c => (
                                    <option key={c.id} value={c.id}>{c.displayName}</option>
                                ))}
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <td style={filterLabelStyle}>Filter by:</td>
                        <td>
                            <select style={filterSelectStyle} value={filterType}
                                onChange={e => handleFilterTypeChange(e.target.value as FilterType)}>
                                <option value="users">Users</option>
                                <option value="methodgroup">Method group</option>
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <td style={filterLabelStyle}>
                            {filterType === "users" ? "User:" : "Method group:"}
                        </td>
                        <td>
                            <select style={filterSelectStyle} value={selectedId}
                                onChange={e => setSelectedId(Number(e.target.value))}>
                                {options.map(o => (
                                    <option key={o.id} value={o.id}>{o.displayName}</option>
                                ))}
                            </select>
                        </td>
                    </tr>
                </tbody>
            </table>

            {/* ── Grid ── */}
            <div style={{ marginTop: "16px" }}>
                <table style={gridStyle}>
                    <thead>
                        <tr style={headerRowStyle}>
                            <th style={searchThStyle}>
                                <input
                                    type="text"
                                    placeholder="Search…"
                                    value={searchText}
                                    onChange={e => setSearchText(e.target.value)}
                                    onKeyDown={e => { if (e.key === "Enter") setAppliedSearch(searchText); }}
                                    style={searchInputStyle}
                                />
                                <button style={searchBtnStyle}
                                    onClick={() => setAppliedSearch(searchText)}>
                                    Search
                                </button>
                            </th>
                            <th style={checkThStyle}>Read</th>
                            <th style={checkThStyle}>Manage</th>
                        </tr>
                    </thead>
                    <tbody>
                        {loading ? (
                            <tr><td colSpan={3} style={{ padding: "12px", color: "#888" }}>Loading…</td></tr>
                        ) : filteredMatrix.length === 0 ? (
                            <tr><td colSpan={3} style={{ padding: "12px", color: "#999" }}>No data found.</td></tr>
                        ) : filteredMatrix.map(row => (
                            <tr key={`${row.userId}-${row.methodGroupId}`} style={dataRowStyle}>
                                <td style={nameTdStyle}>
                                    {filterType === "users" ? row.methodGroupName : row.userDisplayName}
                                </td>
                                <td style={checkTdStyle}>
                                    <input type="checkbox" checked={row.allowRead}
                                        onChange={e => handleToggle(row.userId, row.methodGroupId, "read", e.target.checked)} />
                                </td>
                                <td style={checkTdStyle}>
                                    <input type="checkbox" checked={row.allowManage}
                                        onChange={e => handleToggle(row.userId, row.methodGroupId, "manage", e.target.checked)} />
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

const filterTableStyle:  React.CSSProperties = { borderCollapse: "collapse", marginBottom: "4px" };
const filterLabelStyle:  React.CSSProperties = { paddingRight: "12px", fontWeight: 600, whiteSpace: "nowrap", textAlign: "right", paddingBottom: "6px" };
const filterSelectStyle: React.CSSProperties = { padding: "4px 8px", border: "1px solid #ccc", borderRadius: "4px", minWidth: "240px", marginBottom: "6px" };

const gridStyle:      React.CSSProperties = { width: "100%", borderCollapse: "collapse", fontSize: "13px" };
const headerRowStyle: React.CSSProperties = { backgroundColor: "#e8e8e8" };
const searchThStyle:  React.CSSProperties = { padding: "6px 10px", textAlign: "left", borderBottom: "1px solid #ccc" };
const checkThStyle:   React.CSSProperties = { padding: "6px 16px", textAlign: "center", borderBottom: "1px solid #ccc", fontWeight: 600, width: "70px" };
const dataRowStyle:   React.CSSProperties = { borderBottom: "1px solid #eee" };
const nameTdStyle:    React.CSSProperties = { padding: "6px 10px" };
const checkTdStyle:   React.CSSProperties = { padding: "6px", textAlign: "center" };

const searchInputStyle: React.CSSProperties = { padding: "3px 6px", border: "1px solid #ccc", borderRadius: "3px", marginRight: "6px", width: "140px" };
const searchBtnStyle:   React.CSSProperties = { padding: "3px 10px", border: "1px solid #999", borderRadius: "3px", backgroundColor: "#f0f0f0", cursor: "pointer" };
