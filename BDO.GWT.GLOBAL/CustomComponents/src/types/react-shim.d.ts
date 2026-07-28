declare module 'react' {
    const React: any;

    export = React;
    export as namespace React;

    export type ChangeEvent<T = Element> = {
        target: T & { value: string };
    };

    export function useEffect(effect: (...args: any[]) => any, deps?: readonly any[]): void;
    export function useMemo<T>(factory: () => T, deps: readonly any[]): T;
    export function useState<T>(initialState: T): [T, (value: T) => void];
}

declare module 'react/jsx-runtime' {
    export const Fragment: any;
    export function jsx(type: any, props: any, key?: any): any;
    export function jsxs(type: any, props: any, key?: any): any;
}

declare namespace JSX {
    interface IntrinsicElements {
        [elemName: string]: any;
    }
}