import { IEditorState } from "../util/umbraco/editorstate";

export interface INodeNameSyncScope extends angular.IScope {

    model: IModel;
    editorState: IEditorState;
}

export interface IModel {
    value: string;
}