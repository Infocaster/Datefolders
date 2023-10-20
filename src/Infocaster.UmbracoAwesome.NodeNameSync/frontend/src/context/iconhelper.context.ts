import { createContext } from "@lit/context";
import type { IIconHelper } from "../util/umbraco/icon.service";
export type { IIconHelper } from "../util/umbraco/icon.service";
export const iconHelperKey = 'iconHelper';
export const iconHelperContext = createContext<IIconHelper>(iconHelperKey);