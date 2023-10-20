import { createContext } from "@lit/context";
import { INodeNameSyncScope } from "../models/scope.model";

export const scopeContextKey = "scope";
export const scopeContext = createContext<INodeNameSyncScope>(scopeContextKey);