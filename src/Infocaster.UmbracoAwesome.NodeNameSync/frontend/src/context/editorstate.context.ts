import { createContext } from "@lit/context";
import type { IEditorStateProvider } from "../models/editorstate.model";
export type { IEditorStateProvider } from "../models/editorstate.model";
export const editorStateContextKey = 'editorState';
export const editorStateContext = createContext<IEditorStateProvider>(editorStateContextKey);