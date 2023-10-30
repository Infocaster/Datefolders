import { LitElement, html } from "lit";
import { customElement } from "lit/decorators.js";
import { AngularBridgeMixin } from "../util/bridge/angularbridge.mixin";
import './components/content.lit';

export const nodeNameSyncTag = 'node-name-sync';

@customElement(nodeNameSyncTag)
export class NodeNameSync extends AngularBridgeMixin(LitElement, html`<node-name-sync-content></node-name-sync-content>`) {

}