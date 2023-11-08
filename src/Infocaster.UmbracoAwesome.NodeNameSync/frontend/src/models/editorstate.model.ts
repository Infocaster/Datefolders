export interface IEditorStateProvider {

    getCurrent: () => IEditorState;
}

export interface IEditorState {

    variants: IEditorStateVariation[];
    id: number;
}

export interface IEditorStateVariation {
    
    active: boolean;
    name: string;
}