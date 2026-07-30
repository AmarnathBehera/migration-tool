import React, { useState } from "react";
import { Button, InputBase } from "@kentico/xperience-admin-components";
import { usePageCommand } from "@kentico/xperience-admin-base";

type GdprCommand = 'FetchTextData' | 'FetchXmlData' | 'Delete';

console.log("CustomActionButton loaded");

const Commands: Record<GdprCommand, GdprCommand> = {
    FetchTextData: 'FetchTextData',
    FetchXmlData: 'FetchXmlData',
    Delete: 'Delete',
};

interface GdprCommandData {
    EmailAddress: string;
    Results: string;
}

interface CustomActionButtonProps {
    commandName?: GdprCommand;
    commandType?: GdprCommand;
    showSelector?: boolean;
}

export const CustomActionButton = ({
    commandName,
    commandType,
    showSelector = true
}: CustomActionButtonProps) => {
    alert("CustomActionButton rendered");
    const initialCommand: GdprCommand = commandName ?? commandType ?? 'FetchTextData';
    const [selectedCommand, setSelectedCommand] = useState<GdprCommand>(initialCommand);
    const [emailAddress, setEmailAddress] = useState("");

    const { execute: invokeFetchText } = usePageCommand<void, GdprCommandData>(Commands.FetchTextData);
    const { execute: invokeFetchXml } = usePageCommand<void, GdprCommandData>(Commands.FetchXmlData);
    const { execute: invokeDelete } = usePageCommand<void, GdprCommandData>(Commands.Delete);

    const invokeCommand = (command: GdprCommand) => {
        const data: GdprCommandData = { EmailAddress: emailAddress, Results: "" };
        console.log("Sending payload:", data);
        switch (command) {
            case 'FetchTextData':
                return invokeFetchText(data);
            case 'FetchXmlData':
                return invokeFetchXml(data);
            case 'Delete':
                return invokeDelete(data);
        }
    };

    const commandLabels: Record<GdprCommand, string> = {
        FetchTextData: 'Search to Text',
        FetchXmlData: 'Search to XML',
        Delete: 'Delete User',
    };

    return (
        <div>
            <InputBase
                value={emailAddress}
                onChange={(event) => {
                    setEmailAddress(event.target.value);
                }}
            />
            {showSelector && (
                <select
                    value={selectedCommand}
                    onChange={e => setSelectedCommand(e.target.value as GdprCommand)}
                    title="Select command"
                >
                    <option value="FetchTextData">Search to Text</option>
                    <option value="FetchXmlData">Search to XML</option>
                    <option value="Delete">Delete User</option>
                </select>
            )}
            <Button label={commandLabels[selectedCommand]} onClick={() => invokeCommand(selectedCommand)} />
        </div>
    );
};
