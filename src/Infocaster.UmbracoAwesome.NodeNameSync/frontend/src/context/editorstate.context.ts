import { createContext } from "@lit/context";
import type { IEditorStateProvider } from "../util/umbraco/editorstate";
export type { IEditorStateProvider } from "../util/umbraco/editorstate";
export const editorStateContextKey = 'editorState';
export const editorStateContext = createContext<IEditorStateProvider>(editorStateContextKey);