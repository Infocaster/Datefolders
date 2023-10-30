import { NodeNameSync, nodeNameSyncTag } from "./main.lit";
import { editorStateContext, editorStateContextKey } from "../context/editorstate.context";
import { scopeContext, scopeContextKey } from "../context/scope.context";

ngNodeNameSync.alias = "ngNodeNameSync";
ngNodeNameSync.$inject = ["editorState"]
export function ngNodeNameSync(editorState: any): angular.IDirective {

    return {
        restrict: 'E',
        link: function (_scope, element) {

            let mainElement = document.createElement(nodeNameSyncTag) as NodeNameSync;
            
            mainElement.SetContext(editorState, editorStateContext, editorStateContextKey);
            mainElement.SetContext(_scope, scopeContext, scopeContextKey);
            element[0].appendChild(mainElement);
        }
    };
}