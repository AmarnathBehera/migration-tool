import React, { useState } from "react";
import { Button, TextBox } from "@kentico/xperience-admin-components";
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

    const { execute: invokeFetchText } = usePageCommand(Commands.FetchTextData);
    const { execute: invokeFetchXml } = usePageCommand(Commands.FetchXmlData);
    const { execute: invokeDelete } = usePageCommand(Commands.Delete);

    // Key must match the C# command parameter name ("data").
    const invokeCommand = (command: GdprCommand) => {
        const data: GdprCommandData = { EmailAddress: emailAddress, Results: "" };
        console.log("Sending payload:", data);
        switch (command) {
            case 'FetchTextData':
                return invokeFetchText({ parameter: data });
            case 'FetchXmlData':
                return invokeFetchXml({ parameter: data });
            case 'Delete':
                return invokeDelete({ parameter: data });
        }
    };

    const commandLabels: Record<GdprCommand, string> = {
        FetchTextData: 'Search to Text',
        FetchXmlData: 'Search to XML',
        Delete: 'Delete User',
    };

    return (
        <div>
            <TextBox value={emailAddress} onChange={e => setEmailAddress(e.target.value)} />
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
