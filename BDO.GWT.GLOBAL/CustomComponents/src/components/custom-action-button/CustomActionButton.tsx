import React, { useState } from "react";
import { Button, InputBase } from "@kentico/xperience-admin-components";
import { useFormComponentCommandProvider, FormComponentProps } from "@kentico/xperience-admin-base";

type GdprCommand = 'FetchTextData' | 'FetchXmlData' | 'Delete';

interface FormCommandResponse {
    messages?: Array<{ level: string; message: string }>;
}

export const CustomActionButton = (props: FormComponentProps) => {
    const [emailAddress, setEmailAddress] = useState("");
    const [results, setResults] = useState("");
    const [errorMessage, setErrorMessage] = useState("");

    const { executeCommand } = useFormComponentCommandProvider();
    interface GdprCommandResult {
        success: boolean;
        message: string;
    }

    const invokeCommand = async (commandName: GdprCommand) => {
        setErrorMessage("");

        // props itself satisfies FieldInfo (FormComponentProps extends FieldInfo) —
        // no separate "fieldInfo" prop exists to destructure.
        const result = await executeCommand<GdprCommandResult, string>(
            props,
            commandName,
            emailAddress
        );

        console.log("executeCommand result:", result);

        if (commandName === "Delete") {
            setResults("");
        }

        if (!result) return;

        if (result.success) {
            setResults(result.message);
        } else {
            setErrorMessage(result.message);
        }
    };

    const commandLabels: Record<GdprCommand, string> = {
        FetchTextData: 'Search to Text',
        FetchXmlData: 'Search to XML',
        Delete: 'Delete User',
    };

    return (
        <div>
            <label>
                Email Address:
                <InputBase
                    value={emailAddress}
                    onChange={(event) => setEmailAddress(event.target.value)}
                />
            </label>

            <Button label={commandLabels.FetchTextData} onClick={() => invokeCommand("FetchTextData")} />
            <Button label={commandLabels.FetchXmlData} onClick={() => invokeCommand("FetchXmlData")} />
            <Button label={commandLabels.Delete} onClick={() => invokeCommand("Delete")} />

            <br />
            <label>
                Results:
                <textarea
                    value={results}
                    readOnly
                    rows={10}
                    style={{ width: "100%", minHeight: "250px", padding: "10px", resize: "vertical" }}
                />
            </label>

            {errorMessage && <div style={{ color: "red" }}>{errorMessage}</div>}
        </div>
    );
};