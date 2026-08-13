import React, { useState, useEffect, useCallback, useMemo } from "react";
import { Button, Checkbox, InputBase, Spinner } from "@kentico/xperience-admin-components";
import { useFormComponentCommandProvider } from "@kentico/xperience-admin-base";
import type { ApiPermissionsProps, FilterType, InitResult, LoadMatrixResult, MatrixRow, MethodGroupOption, ToggleResult, UserOption } from "@model/apiPermissions";

const selectClasses = "w-full py-2.5 pl-4 pr-9 rounded-full text-sm leading-snug outline-none appearance-none border box-border transition-colors duration-200 text-label select-chevron bg-white cursor-pointer border-[#8C8C8C] hover:border-black";

export const ApiPermissionsControl = (props: ApiPermissionsProps) => {
    const webChannelSettingId = props.webChannelSettingId ?? 0;

    const [users, setUsers] = useState<UserOption[]>([]);
    const [methodGroups, setMethodGroups] = useState<MethodGroupOption[]>([]);
    const [matrix, setMatrix] = useState<MatrixRow[]>([]);

    const [filterType, setFilterType] = useState<FilterType>("users");
    const [selectedId, setSelectedId] = useState<number>(0);
    const [searchText, setSearchText] = useState<string>("");
    const [appliedSearch, setAppliedSearch] = useState<string>("");
    const [loading, setLoading] = useState(false);
    const [initialized, setInitialized] = useState(false);

    const { executeCommand } = useFormComponentCommandProvider();

    useEffect(() => {
        (async () => {
            const result = await executeCommand<InitResult, string>(props, "Initialize", "");
            if (!result) return;
            setUsers(result.users ?? []);
            setMethodGroups(result.methodGroups ?? []);
            setSelectedId(result.users?.[0]?.id ?? 0);
            setInitialized(true);
        })();
    }, []);

    const loadMatrix = useCallback(async (ft: FilterType, sid: number) => {
        if (sid <= 0) return;
        setLoading(true);
        const result = await executeCommand<LoadMatrixResult, string>(
            props, "LoadMatrix",
            JSON.stringify({ filterType: ft, selectedId: sid, webChannelSettingId })
        );
        setMatrix(result?.rows ?? []);
        setLoading(false);
    }, [executeCommand, props, webChannelSettingId]);

    useEffect(() => {
        if (initialized) loadMatrix(filterType, selectedId);
    }, [filterType, selectedId, initialized]);

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
        const current = matrix.find(r => r.userId === userId && r.methodGroupId === methodGroupId);
        const allowRead = permType === "read" ? allow : (current?.allowRead ?? false);
        const allowManage = permType === "manage" ? allow : (current?.allowManage ?? false);

        setMatrix(prev => prev.map(row =>
            row.userId === userId && row.methodGroupId === methodGroupId
                ? { ...row, allowRead, allowManage }
                : row
        ));
        await executeCommand<ToggleResult, string>(
            props, "TogglePermission",
            JSON.stringify({ userId, methodGroupId, allowRead, allowManage, webChannelSettingId })
        );
    };

    const options = useMemo(() => filterType === "users" ? users : methodGroups, [filterType, users, methodGroups]);

    const filteredMatrix = useMemo(() =>
        appliedSearch.trim()
            ? matrix.filter(row => {
                const name = filterType === "users" ? row.methodGroupName : row.userDisplayName;
                return name.toLowerCase().includes(appliedSearch.toLowerCase());
            })
            : matrix,
        [matrix, appliedSearch, filterType]
    );

    return (
        <div className="font-display">
            <div className="mb-4">
                <label className="block mb-2 text-sm font-normal text-label leading-4">
                    Filter by
                </label>
                <select
                    value={filterType}
                    onChange={e => handleFilterTypeChange(e.target.value as FilterType)}
                    className={selectClasses}
                >
                    <option value="users">Users</option>
                    <option value="methodgroup">Method group</option>
                </select>
            </div>

            <div className="mb-6">
                <label className="block mb-2 text-sm font-normal text-label leading-4">
                    {filterType === "users" ? "User" : "Method group"}
                </label>
                <select
                    value={selectedId}
                    onChange={e => setSelectedId(Number(e.target.value))}
                    className={selectClasses}
                >
                    {options.map(o => (
                        <option key={o.id} value={o.id}>{o.displayName}</option>
                    ))}
                </select>
            </div>

            {filterType === "methodgroup" && (
                <div className="flex items-center gap-2 mb-4">
                    <div className="flex-1 max-w-xs" onKeyDown={e => { if (e.key === "Enter") setAppliedSearch(searchText); }}>
                        <InputBase
                            value={searchText}
                            onChange={e => setSearchText(e.target.value)}
                            placeholder="Search…"
                        />
                    </div>
                    <Button label="Search" onClick={() => setAppliedSearch(searchText)} />
                    {appliedSearch && (
                        <Button label="Clear" onClick={() => { setAppliedSearch(""); setSearchText(""); }} />
                    )}
                </div>
            )}

            <table className="w-full border-collapse text-sm">
                <thead>
                    <tr className="bg-[#e8e8e8]">
                        <th className="py-2 px-3 text-left border-b border-separator font-semibold text-heading">&nbsp;</th>
                        <th className="py-2 px-4 text-center border-b border-separator font-semibold text-heading w-[80px]">Read</th>
                        <th className="py-2 px-4 text-center border-b border-separator font-semibold text-heading w-[80px]">Manage</th>
                    </tr>
                </thead>
                <tbody>
                    {loading ? (
                        <tr>
                            <td colSpan={3} className="p-4">
                                <div className="flex items-center gap-2 text-muted">
                                    <Spinner />
                                    <span>Loading…</span>
                                </div>
                            </td>
                        </tr>
                    ) : filteredMatrix.length === 0 ? (
                        <tr>
                            <td colSpan={3} className="p-4 text-muted">No data found.</td>
                        </tr>
                    ) : filteredMatrix.map(row => (
                        <tr key={`${row.userId}-${row.methodGroupId}`} className="border-b border-[#eee]">
                            <td className="py-1.5 px-3 text-body">
                                {filterType === "users" ? row.methodGroupName : row.userDisplayName}
                            </td>
                            <td className="py-1.5 px-4 text-center">
                                <Checkbox
                                    checked={row.allowRead}
                                    onChange={(_, checked) => handleToggle(row.userId, row.methodGroupId, "read", checked)}
                                />
                            </td>
                            <td className="py-1.5 px-4 text-center">
                                <Checkbox
                                    checked={row.allowManage}
                                    onChange={(_, checked) => handleToggle(row.userId, row.methodGroupId, "manage", checked)}
                                />
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};
