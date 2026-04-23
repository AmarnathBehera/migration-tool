(function (pageBuilder) {
    var richTextEditor = pageBuilder.richTextEditor = pageBuilder.richTextEditor || {};
    var configurations = richTextEditor.configurations = richTextEditor.configurations || {};
    // Overrides the "default" configuration of the Rich text widget, assuming you have not specified a different configuration
    // The below configuration replace all available fonts with a Proxima Nova. If we need more fonts then we have to add new configuration or update existing default.

    const common = {
        paragraphFormat: {
            N: "Normal",
            H1: "Headline 1",
            H2: "Headline 2",
            H3: "Headline 3",
            H4: "Headline 4",
            H5: "Headline 5",
            H6: "Headline 6",
        },
        paragraphStyles: {
            'rich-text__body-regular': "Body Regular",
            'rich-text__body-regular--bold': "Body Regular Bold",
            'rich-text__body-small': "Body Small",
            'rich-text__body-small--bold': "Body Small Bold",
            'rich-text__badge': "Badge",
            'rich-text__caption': "Caption",
            'rich-text__label': "Label",
            'rich-text__link': "Link",
            'rich-text__link--small': "Link Small",
            'rich-text__cta': "CTA",
            'rich-text__cta--large': "CTA Large",
            'rich-text__cta--small': "CTA Small",
        },
        fontFamily: {
            '"Proxima Nova", "Helvetica Neue", Helvetica, Arial, sans-serif': 'Proxima Nova'
        },
        colorsText: [
            '#E81A3B', '#333333', '#0062b8', '#5B6E7F', '#0062B8', '#FFFFFF', '#666666',
        ],
        colorsBackground: [
            '#E7E7E7', '#F2F2F2', '#FFFFFF', '#5B6E7F', '#0062B8', '#333333', '#404040',
        ],
    };

    configurations["default"] = {
        toolbarVisibleWithoutSelection: true,
        listAdvancedTypes: false,
        paragraphFormat: common.paragraphFormat,
        paragraphStyles: common.paragraphStyles,
        tabSpaces: 4,
        fontFamily: common.fontFamily,
        colorsText: common.colorsText,
        colorsHEXInput: false,
        colorsBackground: common.colorsBackground,
        wordCounterCount: true,
        charCounterCount: true,
    };
    configurations["quoteText"] = {
        toolbarVisibleWithoutSelection: true,
        toolbarButtons: {
            moreText: {
                buttons: [
                    'bold', 'underline',
                ],
                buttonsVisible: 3,
            },
            moreMisc: {
                buttons: [
                    'undo', 'redo', 'selectAll', 'help',
                ],
                buttonsVisible : 4,
            }
        },
        tabSpaces: 4,
        shortcutsEnabled: [
            'bold', 'underline', 'undo', 'redo',
        ],
        htmlAllowedTags: [
            'p', 'strong', 'u', 'br',
        ],
        htmlAllowedAttrs: [],
        pasteDeniedTags: [
            'a', 'table', 'thead', 'tbody', 'tfoot', 'tr', 'td', 'th', 'col', 'colgroup', 'caption', 'em', 'i', 'q',
            'iframe', 'video', 'audio', 'source', 'embed', 'object', 'canvas', 'picture',
            'code', 'pre',
            'form', 'input', 'textarea', 'button', 'select', 'option', 'label', 'fieldset', 'legend',
            'style', 'script', 'link', 'meta', 'title', 'base',
            's', 'strike', 'sub', 'sup', 'font', 'span',
            'ul', 'ol', 'li', 'blockquote',
            'small', 'big', 'mark', 'del', 'ins',
        ],
        imagePaste: false,
        wordPasteModal: false,
        wordPasteKeepFormatting: false,
        helpSets: [
            {
                title: 'Common actions',
                commands: [
                    { val: 'OSkeyC', desc: 'Copy' },
                    { val: 'OSkeyX', desc: 'Cut' },
                    { val: 'OSkeyV', desc: 'Paste' },
                    { val: 'OSkeyZ', desc: 'Undo' },
                    { val: 'OSkeyShift+Z', desc: 'Redo' },
                ]
            },
            {
                title: 'Basic Formatting',
                commands: [
                    { val: 'OSkeyA', desc: 'Select All' },
                    { val: 'OSkeyB', desc: 'Bold' },
                    { val: 'OSkeyU', desc: 'Underline' },
                ]
            }
        ],
    };
    configurations["keyTakeawaysText"] = {
        toolbarVisibleWithoutSelection: true,
        toolbarButtons: {
            moreText: {
                buttons: [
                    'bold', 'italic', 'underline',
                    'strikeThrough', 'subscript', 'superscript',
                    'fontFamily', 'fontSize',
                    'textColor', 'backgroundColor',
                    'inlineClass', 'inlineStyle', 'clearFormatting'
                ],
                buttonsVisible: 3,
            },
            moreParagraph: {
                buttons: [
                    'formatUL', 'formatOL', 'paragraphFormat',
                    'paragraphStyle', 'lineHeight', 'outdent', 'indent', 'quote'
                ],
                buttonsVisible: 2,
            },
            moreRich: {
                buttons: [
                    'specialCharacters', 'insertHR'
                ],
                buttonsVisible: 2,
            },
            moreMisc: {
                buttons: [
                    'undo', 'redo', 'selectAll', 'html', 'help',
                ],
                buttonsVisible: 2,
            },
        },
        listAdvancedTypes: false,
        paragraphFormat: common.paragraphFormat,
        paragraphStyles: common.paragraphStyles,
        tabSpaces: 4,
        fontFamily: common.fontFamily,
        colorsText: common.colorsText,
        colorsHEXInput: false,
        colorsBackground: common.colorsBackground,
    };
    configurations["description"] = {
        toolbarVisibleWithoutSelection: true,
        toolbarButtons: {
            moreText: {
                buttons: [
                    'bold', 'italic', 'underline',
                    'strikeThrough', 'subscript', 'superscript',
                    'fontFamily', 'fontSize',
                    'textColor', 'backgroundColor',
                    'inlineClass', 'inlineStyle', 'clearFormatting'
                ],
                buttonsVisible: 3,
            },
            moreParagraph: {
                buttons: [
                    'formatUL', 'formatOL', 'paragraphFormat',
                    'alignLeft', 'alignCenter', 'alignRight', 'alignJustify',
                    'paragraphStyle', 'lineHeight', 'outdent', 'indent', 'quote'
                ],
                buttonsVisible: 2,
            },
            moreRich: {
                buttons: [
                    'specialCharacters', 'insertHR'
                ],
                buttonsVisible: 2,
            },
            moreMisc: {
                buttons: [
                    'undo', 'redo', 'selectAll', 'html', 'help',
                ],
                buttonsVisible: 2,
            },
        },
        listAdvancedTypes: false,
        paragraphFormat: common.paragraphFormat,
        paragraphStyles: common.paragraphStyles,
        tabSpaces: 4,
        fontFamily: common.fontFamily,
        colorsText: common.colorsText,
        colorsHEXInput: false,
        colorsBackground: common.colorsBackground,
        pasteDeniedTags: [
            'video', 'picture', 'img', 'table',
        ],
        imagePaste: false,
    };
})(window.kentico.pageBuilder);