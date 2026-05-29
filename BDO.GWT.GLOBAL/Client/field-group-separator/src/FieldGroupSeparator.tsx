import React from "react";

export const FieldGroupSeparatorFormComponent = (props: any) => {
    const headerText = props.label || "Section";
    const description: string = props.explanationText || "";

    return React.createElement(
        "div",
        { style: { marginTop: 8 } },
        React.createElement(
            "div",
            {
                style: {
                    display: "flex",
                    alignItems: "center",
                    gap: 12,
                    marginBottom: description.length > 0 ? 8 : 12
                }
            },
            React.createElement(
                "div",
                {
                    style: {
                        fontSize: 16,
                        fontWeight: "bold",
                        color: "black"
                    }
                },
                headerText
            )
        ),
        description.length > 0
            ? React.createElement(
                "div",
                {
                    style: {
                        fontSize: 12,
                        color: "black",
                        lineHeight: 1.4,
                        marginBottom: 14
                    }
                },
                description
            )
            : null,
        React.createElement("div", {
            "aria-hidden": "true",
            style: {
                height: 1,
                width: "100%",
                backgroundColor: "var(--color-border-default)"
            }
        })
    );
};
