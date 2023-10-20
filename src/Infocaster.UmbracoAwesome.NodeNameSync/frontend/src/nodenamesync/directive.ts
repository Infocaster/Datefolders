import { NodeNameSync, nodeNameSyncTag } from "./main.lit";
import { IIconHelper, iconHelperContext, iconHelperKey } from "../context/iconhelper.context";
import { editorStateContext, editorStateContextKey } from "../context/editorstate.context";
import { scopeContext, scopeContextKey } from "../context/scope.context";

ngNodeNameSync.alias = "ngNodeNameSync";
ngNodeNameSync.$inject = ["iconHelper", "editorState"]
export function ngNodeNameSync(iconHelper: IIconHelper, editorState: any): angular.IDirective {

    return {
        restrict: 'E',
        link: function (_scope, element) {

            let mainElement = document.createElement(nodeNameSyncTag) as NodeNameSync;
            
            mainElement.SetContext(iconHelper, iconHelperContext, iconHelperKey);
            mainElement.SetContext(editorState, editorStateContext, editorStateContextKey);
            mainElement.SetContext(_scope, scopeContext, scopeContextKey);
            element[0].appendChild(mainElement);
        }
    };
}