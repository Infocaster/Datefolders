import { consume } from "@lit/context";
import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";
import { IEditorStateProvider, IEditorStateVariation } from "../../util/umbraco/editorstate";
import { editorStateContext } from "../../context/editorstate.context";
import { ensureServiceExists } from "../../util/ensure";
import { scopeContext } from "../../context/scope.context";
import { INodeNameSyncScope } from "../../models/scope.model";

export const nodeNameSyncContentTag = "node-name-sync-content";

@customElement(nodeNameSyncContentTag)
export class NodeNameSyncContent extends LitElement {
  @consume({ context: editorStateContext })
  private editorState?: IEditorStateProvider;

  @consume({ context: scopeContext })
  private scope?: INodeNameSyncScope;

  @state()
  private editorStateCurrentCulture?: IEditorStateVariation;

  @state()
  private inSync: boolean = false;

  @state()
  private syncFieldValue: string = "";

  @state()
  private nameChangedLast: boolean = true;

  async connectedCallback(): Promise<void> {
    super.connectedCallback();

    ensureServiceExists(this.editorState, "editor state");
    ensureServiceExists(this.scope, "scope");

    const currentState = this.editorState.getCurrent();
    this.scope.editorState = currentState;
    this.editorStateCurrentCulture = currentState.variants.find((v) => v.active);
    this.inSync = this.editorStateCurrentCulture?.name === this.scope.model.value;
    this.syncFieldValue = this.scope.model.value;

    this.scope.$watch(() => {
      return this.scope?.editorState.variants.find(element => element.active)?.name;
    }, (value) => {
      this.fromNameToModel(value);
    });
  }

  // From node name to model
  fromNameToModel(newValue?: string) {
    if (this.inSync && newValue) {
      this.syncFieldValue = newValue;
      this.updateModel();
    }
    this.nameChangedLast = true;
  }

  // From model to node name
  fromModelToName(newValue: string) {
    if (this.inSync && this.editorStateCurrentCulture) {
      this.editorStateCurrentCulture.name = newValue;
    }

    this.nameChangedLast = false;
    this.updateModel();
  }

  toggleSync() {
    this.inSync = !this.inSync;

    if (this.nameChangedLast && this.editorStateCurrentCulture != null) {
      this.fromNameToModel(this.editorStateCurrentCulture.name);
    } else {
      if (this.scope) {
        this.fromModelToName(this.scope.model.value);
      }
    }
  }

  updateModel() {
    if (this.scope) {
      this.scope.model.value = this.syncFieldValue;
    }
  }

  onKeyDown(event: any) {
    if (
      event.key === " " ||
      event.key === "Enter" ||
      event.key === "Spacebar"
    ) {
      // Prevent the default action to stop scrolling when space is pressed
      event.preventDefault();
      this.toggleSync();
    }
  }

  onChange(e: any) {
    this.syncFieldValue = e.srcElement.value;
    this.fromModelToName(e.srcElement.value);
  }

  protected render(): unknown {
    return html`
      <uui-input value=${this.syncFieldValue} @input=${this.onChange}></uui-input>
      <uui-button 
            compact="true"
            title="${this.inSync ? "Unlink from page name" : "Link to page name"}"
            role="button"
            aria-label="Link to page name"
            aria-pressed=${this.inSync}
            @click=${this.toggleSync} 
            @keydown=${this.onKeyDown}>
        <uui-icon-registry-essential>
          <uui-icon 
            class="${this.inSync ? "lock" : "lock unlocked"}" 
            name="${this.inSync ? "lock" : "unlock"}">
          </uui-icon>
        </uui-icon-registry-essential>
      </uui-button>
    `;
  }

  public static styles = [
    css`
      uui-input {
        width: auto;
        min-width: 580px;
        float: left;
      }

      .lock {
        text-decoration: none;
        font-size: 1.5rem;

        color: #4fa23c;
        line-height: 2rem;
      }

      .lock.unlocked {
        color: #ee5f5b;
      }

      .lock uui-icon-registry-essential {
        line-height: 2rem;
      }
    `,
  ];
}
