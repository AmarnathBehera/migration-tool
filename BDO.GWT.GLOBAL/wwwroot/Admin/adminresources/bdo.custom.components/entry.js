/*
 * ATTENTION: The "eval" devtool has been used (maybe by default in mode: "development").
 * This devtool is neither made for production nor for readable output files.
 * It uses "eval()" calls to create a separate source file in the browser devtools.
 * If you are trying to read the output file, select a different devtool (https://webpack.js.org/configuration/devtool/)
 * or disable the default devtool with "devtool: false".
 * If you are looking for production-ready output files, see mode: "production" (https://webpack.js.org/configuration/mode/).
 */
System.register(["@kentico/xperience-admin-base","@kentico/xperience-admin-components","react"], function(__WEBPACK_DYNAMIC_EXPORT__, __system_context__) {
	var __WEBPACK_EXTERNAL_MODULE__kentico_xperience_admin_base__ = {};
	var __WEBPACK_EXTERNAL_MODULE__kentico_xperience_admin_components__ = {};
	var __WEBPACK_EXTERNAL_MODULE_react__ = {};
	Object.defineProperty(__WEBPACK_EXTERNAL_MODULE__kentico_xperience_admin_base__, "__esModule", { value: true });
	Object.defineProperty(__WEBPACK_EXTERNAL_MODULE__kentico_xperience_admin_components__, "__esModule", { value: true });
	Object.defineProperty(__WEBPACK_EXTERNAL_MODULE_react__, "__esModule", { value: true });
	return {
		setters: [
			function(module) {
				__WEBPACK_EXTERNAL_MODULE__kentico_xperience_admin_base__["default"] = module["default"] || module;
				Object.keys(module).forEach(function(key) {
					__WEBPACK_EXTERNAL_MODULE__kentico_xperience_admin_base__[key] = module[key];
				});
			},
			function(module) {
				__WEBPACK_EXTERNAL_MODULE__kentico_xperience_admin_components__["default"] = module["default"] || module;
				Object.keys(module).forEach(function(key) {
					__WEBPACK_EXTERNAL_MODULE__kentico_xperience_admin_components__[key] = module[key];
				});
			},
			function(module) {
				__WEBPACK_EXTERNAL_MODULE_react__["default"] = module["default"] || module;
				Object.keys(module).forEach(function(key) {
					__WEBPACK_EXTERNAL_MODULE_react__[key] = module[key];
				});
			}
		],
		execute: function() {
			__WEBPACK_DYNAMIC_EXPORT__(
/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ({

/***/ "./src/styles/index.css"
/*!******************************!*\
  !*** ./src/styles/index.css ***!
  \******************************/
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

eval("{__webpack_require__.r(__webpack_exports__);\n// extracted by mini-css-extract-plugin\n\n\n//# sourceURL=webpack://@bdo/custom-components/./src/styles/index.css?\n}");

/***/ },

/***/ "./entry.tsx"
/*!*******************!*\
  !*** ./entry.tsx ***!
  \*******************/
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

eval("{__webpack_require__.r(__webpack_exports__);\n/* harmony export */ __webpack_require__.d(__webpack_exports__, {\n/* harmony export */   BDOSQLDropdown: () => (/* reexport safe */ _components_custom_dropdown_CustomDropdown__WEBPACK_IMPORTED_MODULE_1__.BDOSQLDropdown),\n/* harmony export */   BDOSQLDropdownFormComponent: () => (/* reexport safe */ _components_custom_dropdown_CustomDropdown__WEBPACK_IMPORTED_MODULE_1__.BDOSQLDropdown),\n/* harmony export */   FieldGroupSeparatorFormComponent: () => (/* reexport safe */ _components_field_group_separator_FieldGroupSeparator__WEBPACK_IMPORTED_MODULE_3__.FieldGroupSeparator),\n/* harmony export */   GDPRControl: () => (/* reexport safe */ _components_gdpr_control_GDPRControl__WEBPACK_IMPORTED_MODULE_2__.GDPRControl),\n/* harmony export */   GDPRControlFormComponent: () => (/* reexport safe */ _components_gdpr_control_GDPRControl__WEBPACK_IMPORTED_MODULE_2__.GDPRControl)\n/* harmony export */ });\n/* harmony import */ var _styles_index_css__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @styles/index.css */ \"./src/styles/index.css\");\n/* harmony import */ var _components_custom_dropdown_CustomDropdown__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @components/custom-dropdown/CustomDropdown */ \"./src/components/custom-dropdown/CustomDropdown.tsx\");\n/* harmony import */ var _components_gdpr_control_GDPRControl__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ~/components/gdpr-control/GDPRControl */ \"./src/components/gdpr-control/GDPRControl.tsx\");\n/* harmony import */ var _components_field_group_separator_FieldGroupSeparator__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @components/field-group-separator/FieldGroupSeparator */ \"./src/components/field-group-separator/FieldGroupSeparator.tsx\");\n\n\n\n\nif (typeof document !== 'undefined') {\n    const cssHref = '/Admin/adminresources/bdo.custom.components/entry.css';\n    if (!document.querySelector(`link[href=\"${cssHref}\"]`)) {\n        const link = document.createElement('link');\n        link.rel = 'stylesheet';\n        link.href = cssHref;\n        document.head.appendChild(link);\n    }\n}\n\n\n\n//# sourceURL=webpack://@bdo/custom-components/./entry.tsx?\n}");

/***/ },

/***/ "./src/components/custom-dropdown/CustomDropdown.tsx"
/*!***********************************************************!*\
  !*** ./src/components/custom-dropdown/CustomDropdown.tsx ***!
  \***********************************************************/
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

eval("{__webpack_require__.r(__webpack_exports__);\n/* harmony export */ __webpack_require__.d(__webpack_exports__, {\n/* harmony export */   BDOSQLDropdown: () => (/* binding */ BDOSQLDropdown)\n/* harmony export */ });\n/* harmony import */ var react__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! react */ \"react\");\n\nconst BDOSQLDropdown = (props) => {\n    const displayLabel = props.fieldLabel || props.label;\n    const isReadOnly = props.disabled === true;\n    return (react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"div\", { className: \"mb-6 font-display\" },\n        displayLabel && (react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"label\", { className: \"block mb-2 text-sm font-normal text-label leading-4\" },\n            props.required && react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"span\", { className: \"text-error mr-0.5\" }, \"*\"),\n            displayLabel)),\n        react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"select\", { value: props.value ?? '', onChange: e => !isReadOnly && props.onChange && props.onChange(e.target.value), disabled: isReadOnly, className: `w-full py-2.5 pl-4 pr-9 rounded-full text-sm leading-snug outline-none appearance-none border box-border transition-colors duration-200 text-label select-chevron ${isReadOnly ? 'bg-[#F5F5F5] border-[#D0D0D0] cursor-default' : `bg-white cursor-pointer ${props.invalid ? 'border-error' : 'border-[#8C8C8C] hover:border-black'}`}` },\n            react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"option\", { value: \"\" }, props.defaultValue ?? 'Choose an option'),\n            props.options?.map((item, index) => (react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"option\", { key: index, value: item.text }, item.text)))),\n        props.explanationText && (react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"div\", { className: \"text-muted text-xs leading-normal mt-1.5 pl-4\" }, props.explanationText)),\n        props.validationMessage && (react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"div\", { className: \"text-error text-xs leading-normal mt-1.5 pl-4 font-medium\" }, props.validationMessage))));\n};\n\n\n//# sourceURL=webpack://@bdo/custom-components/./src/components/custom-dropdown/CustomDropdown.tsx?\n}");

/***/ },

/***/ "./src/components/field-group-separator/FieldGroupSeparator.tsx"
/*!**********************************************************************!*\
  !*** ./src/components/field-group-separator/FieldGroupSeparator.tsx ***!
  \**********************************************************************/
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

eval("{__webpack_require__.r(__webpack_exports__);\n/* harmony export */ __webpack_require__.d(__webpack_exports__, {\n/* harmony export */   FieldGroupSeparator: () => (/* binding */ FieldGroupSeparator)\n/* harmony export */ });\n/* harmony import */ var react__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! react */ \"react\");\n\nconst FieldGroupSeparator = (props = {}) => {\n    const label = (props && props.label) ? String(props.label) : '';\n    const explanationText = (props && props.explanationText) ? String(props.explanationText) : '';\n    return (react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"div\", { className: \"mt-2\" },\n        label && (react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"div\", { className: `text-base font-bold text-heading ${explanationText.length > 0 ? 'mb-2' : 'mb-3'}` }, label)),\n        explanationText.length > 0 && (react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"div\", { className: \"text-xs text-label mb-3.5 leading-normal\" }, explanationText))));\n};\n\n\n//# sourceURL=webpack://@bdo/custom-components/./src/components/field-group-separator/FieldGroupSeparator.tsx?\n}");

/***/ },

/***/ "./src/components/gdpr-control/GDPRControl.tsx"
/*!*****************************************************!*\
  !*** ./src/components/gdpr-control/GDPRControl.tsx ***!
  \*****************************************************/
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

eval("{__webpack_require__.r(__webpack_exports__);\n/* harmony export */ __webpack_require__.d(__webpack_exports__, {\n/* harmony export */   GDPRControl: () => (/* binding */ GDPRControl)\n/* harmony export */ });\n/* harmony import */ var react__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! react */ \"react\");\n/* harmony import */ var _kentico_xperience_admin_components__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @kentico/xperience-admin-components */ \"@kentico/xperience-admin-components\");\n/* harmony import */ var _kentico_xperience_admin_base__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @kentico/xperience-admin-base */ \"@kentico/xperience-admin-base\");\n\n\n\nconst isValidEmail = (email) => /^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$/.test(email.trim());\nconst commandLabels = {\n    FetchTextData: 'Search to Text',\n    FetchXmlData: 'Search to XML',\n    Delete: 'Delete',\n};\nconst GDPRControl = (props) => {\n    const [emailAddress, setEmailAddress] = (0,react__WEBPACK_IMPORTED_MODULE_0__.useState)(\"\");\n    const [results, setResults] = (0,react__WEBPACK_IMPORTED_MODULE_0__.useState)(\"\");\n    const [fetchedEmail, setFetchedEmail] = (0,react__WEBPACK_IMPORTED_MODULE_0__.useState)(\"\");\n    const [errorMessage, setErrorMessage] = (0,react__WEBPACK_IMPORTED_MODULE_0__.useState)(\"\");\n    const [successMessage, setSuccessMessage] = (0,react__WEBPACK_IMPORTED_MODULE_0__.useState)(\"\");\n    const [copied, setCopied] = (0,react__WEBPACK_IMPORTED_MODULE_0__.useState)(false);\n    const { executeCommand } = (0,_kentico_xperience_admin_base__WEBPACK_IMPORTED_MODULE_2__.useFormComponentCommandProvider)();\n    const validateEmailInput = () => {\n        if (!emailAddress.trim()) {\n            setErrorMessage(\"Please enter an email address.\");\n            return false;\n        }\n        if (!isValidEmail(emailAddress)) {\n            setErrorMessage(\"Please enter a valid email address.\");\n            return false;\n        }\n        return true;\n    };\n    const invokeCommand = async (commandName) => {\n        setErrorMessage(\"\");\n        setSuccessMessage(\"\");\n        const result = await executeCommand(props, commandName, emailAddress);\n        if (!result)\n            return;\n        if (result.success) {\n            if (commandName === \"Delete\") {\n                setResults(\"\");\n                setFetchedEmail(\"\");\n                setSuccessMessage(result.message);\n                setTimeout(() => setSuccessMessage(\"\"), 3000);\n            }\n            else {\n                setResults(result.message);\n                setFetchedEmail(emailAddress.trim());\n            }\n        }\n        else {\n            setErrorMessage(result.message);\n        }\n    };\n    const handleDelete = () => {\n        if (!validateEmailInput())\n            return;\n        if (!window.confirm(\"Are you sure you want to delete the personal data for this email address? This action cannot be undone.\"))\n            return;\n        invokeCommand(\"Delete\");\n    };\n    const handleSearch = (commandName) => {\n        if (!validateEmailInput())\n            return;\n        invokeCommand(commandName);\n    };\n    const handleCopy = async () => {\n        if (!results)\n            return;\n        try {\n            await navigator.clipboard.writeText(results);\n            setCopied(true);\n            setTimeout(() => setCopied(false), 3000);\n        }\n        catch (err) {\n            console.error(\"Copy failed:\", err);\n        }\n    };\n    return (react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"div\", { className: \"font-display\" },\n        react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"div\", { className: \"mb-6\" },\n            react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"label\", { className: \"block mb-2 text-sm font-normal text-label leading-4\" }, \"Email Address\"),\n            react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(_kentico_xperience_admin_components__WEBPACK_IMPORTED_MODULE_1__.InputBase, { value: emailAddress, onChange: (event) => {\n                    setEmailAddress(event.target.value);\n                    setFetchedEmail(\"\");\n                } })),\n        react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"div\", { className: \"flex flex-wrap items-center gap-2 mb-4\" },\n            react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(_kentico_xperience_admin_components__WEBPACK_IMPORTED_MODULE_1__.Button, { label: commandLabels.FetchTextData, onClick: () => handleSearch(\"FetchTextData\") }),\n            react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(_kentico_xperience_admin_components__WEBPACK_IMPORTED_MODULE_1__.Button, { label: commandLabels.FetchXmlData, onClick: () => handleSearch(\"FetchXmlData\") }),\n            react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(_kentico_xperience_admin_components__WEBPACK_IMPORTED_MODULE_1__.Button, { label: commandLabels.Delete, onClick: handleDelete, disabled: !results || !fetchedEmail || fetchedEmail !== emailAddress.trim() }),\n            react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(_kentico_xperience_admin_components__WEBPACK_IMPORTED_MODULE_1__.Button, { label: \"Copy\", onClick: handleCopy, disabled: !results }),\n            copied && (react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"span\", { className: \"text-xs text-[#2e7d32] font-medium pl-1\" }, \"\\u2713 Copied to clipboard\"))),\n        errorMessage && (react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"div\", { className: \"text-error/75 text-xs leading-normal mt-1.5 pl-4 font-medium mb-4\" }, errorMessage)),\n        successMessage && (react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"div\", { className: \"text-[#2e7d32] text-xs leading-normal mt-1.5 pl-4 font-medium mb-4\" },\n            \"\\u2713 \",\n            successMessage)),\n        react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"div\", { className: \"mb-2\" },\n            react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"label\", { className: \"block mb-2 text-sm font-normal text-label leading-4\" }, \"Results\"),\n            react__WEBPACK_IMPORTED_MODULE_0__[\"default\"].createElement(\"textarea\", { value: results, readOnly: true, rows: 10, className: \"w-full min-h-[250px] p-3 text-xs leading-relaxed text-body font-mono bg-[#F5F5F5] border border-[#D0D0D0] rounded resize-y outline-none\" }))));\n};\n\n\n//# sourceURL=webpack://@bdo/custom-components/./src/components/gdpr-control/GDPRControl.tsx?\n}");

/***/ },

/***/ "@kentico/xperience-admin-base"
/*!************************************************!*\
  !*** external "@kentico/xperience-admin-base" ***!
  \************************************************/
(module) {

module.exports = __WEBPACK_EXTERNAL_MODULE__kentico_xperience_admin_base__;

/***/ },

/***/ "@kentico/xperience-admin-components"
/*!******************************************************!*\
  !*** external "@kentico/xperience-admin-components" ***!
  \******************************************************/
(module) {

module.exports = __WEBPACK_EXTERNAL_MODULE__kentico_xperience_admin_components__;

/***/ },

/***/ "react"
/*!************************!*\
  !*** external "react" ***!
  \************************/
(module) {

module.exports = __WEBPACK_EXTERNAL_MODULE_react__;

/***/ }

/******/ 	});
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		var cachedModule = __webpack_module_cache__[moduleId];
/******/ 		if (cachedModule !== undefined) {
/******/ 			return cachedModule.exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
/******/ 			// no module.id needed
/******/ 			// no module.loaded needed
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		if (!(moduleId in __webpack_modules__)) {
/******/ 			delete __webpack_module_cache__[moduleId];
/******/ 			var e = new Error("Cannot find module '" + moduleId + "'");
/******/ 			e.code = 'MODULE_NOT_FOUND';
/******/ 			throw e;
/******/ 		}
/******/ 		__webpack_modules__[moduleId](module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/************************************************************************/
/******/ 	/* webpack/runtime/define property getters */
/******/ 	(() => {
/******/ 		// define getter functions for harmony exports
/******/ 		__webpack_require__.d = (exports, definition) => {
/******/ 			for(var key in definition) {
/******/ 				if(__webpack_require__.o(definition, key) && !__webpack_require__.o(exports, key)) {
/******/ 					Object.defineProperty(exports, key, { enumerable: true, get: definition[key] });
/******/ 				}
/******/ 			}
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/hasOwnProperty shorthand */
/******/ 	(() => {
/******/ 		__webpack_require__.o = (obj, prop) => (Object.prototype.hasOwnProperty.call(obj, prop))
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/make namespace object */
/******/ 	(() => {
/******/ 		// define __esModule on exports
/******/ 		__webpack_require__.r = (exports) => {
/******/ 			if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 				Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 			}
/******/ 			Object.defineProperty(exports, '__esModule', { value: true });
/******/ 		};
/******/ 	})();
/******/ 	
/************************************************************************/
/******/ 	
/******/ 	// startup
/******/ 	// Load entry module and return exports
/******/ 	// This entry module can't be inlined because the eval devtool is used.
/******/ 	var __webpack_exports__ = __webpack_require__("./entry.tsx");
/******/ 	
/******/ 	return __webpack_exports__;
/******/ })()

			);
		}
	};
});