import { IEditorState } from "./editorstate.model";

export interface INodeNameSyncScope extends angular.IScope {

    model: IModel;
    editorState: IEditorState;
}

export interface IModel {
    value: string;
}