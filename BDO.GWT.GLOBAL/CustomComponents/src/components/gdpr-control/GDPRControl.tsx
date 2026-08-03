import React, { useState } from "react";
import { Button, InputBase } from "@kentico/xperience-admin-components";
import { useFormComponentCommandProvider, FormComponentProps } from "@kentico/xperience-admin-base";

type GdprCommand = 'FetchTextData' | 'FetchXmlData' | 'Delete';

interface GdprCommandResult {
    success: boolean;
    message: string;
}

export const GDPRControl = (props: FormComponentProps) => {
    const [emailAddress, setEmailAddress] = useState("");
    const [results, setResults] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const [copyLabel, setCopyLabel] = useState("Copy");

    const { executeCommand } = useFormComponentCommandProvider();

    const invokeCommand = async (commandName: GdprCommand) => {
        setErrorMessage("");

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

    const handleCopy = async () => {
        if (!results) return;

        try {
            await navigator.clipboard.writeText(results);
            setCopyLabel("Copied!");
        } catch (err) {
            console.error("Copy failed:", err);
            setCopyLabel("Copy failed");
        }

        // Reset the label back after a moment.
        setTimeout(() => setCopyLabel("Copy"), 1500);
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
            <Button label={copyLabel} onClick={handleCopy} disabled={!results} />
            {errorMessage && <div style={{ color: "red" }}>{errorMessage}</div>}
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
        </div>
    );
};