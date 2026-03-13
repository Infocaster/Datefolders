import { LitElement, css, customElement, html, ifDefined, property, query, state, } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { isNameablePropertyDatasetContext, UMB_PROPERTY_DATASET_CONTEXT, type UmbNameablePropertyDatasetContext, } from "@umbraco-cms/backoffice/property";
import { UmbChangeEvent } from "@umbraco-cms/backoffice/event";
import { UUIInputElement, type UUIInterfaceColor, } from "@umbraco-cms/backoffice/external/uui";

@customElement("ic-nodenamesync")
export class NodeNameSyncPropertyEditorUi extends UmbElementMixin(LitElement) {
  @property({ type: String })
  public value?: string = "";

  private setValue(value?: string) {
    this.value = value;
        this.dispatchEvent(new UmbChangeEvent());
  }

  datasetContext?: UmbNameablePropertyDatasetContext;

  @state()
  name?: string;

  @state()
  syncEnabled: boolean = false;

  @query("uui-input", true)
  input?: UUIInputElement;

  constructor() {
    super();
    this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (instance) => {
      if (!instance) return;

      this.observe(instance.name, (name) => this._onNameChange(name));

      if (isNameablePropertyDatasetContext(instance)) {
        this.datasetContext = instance;
      }
    });
  }

  connectedCallback(): void {
    super.connectedCallback();
    this.syncEnabled = this.value === this.name;
  }

  private _onNameChange(name?: string) {
    this.name = name;
    if (this.syncEnabled && this.name !== this.value) {
      this.setValue(this.name);
    }
  }

  private _onInput() {
    if (this.input) {
      this.setValue(this.input.value.toString());
      if (this.syncEnabled && this.datasetContext) {
        this.datasetContext.setName(this.value ?? "");
      }
    }
  }

  private _toggleSync() {
    this.syncEnabled = !this.syncEnabled;
    if (this.syncEnabled) {
      this.setValue(this.name);
    }
  }

  protected render(): unknown {
    return html`
      <uui-input @input=${this._onInput} value=${ifDefined(this.value)}
        >${this.renderButton()}</uui-input
      >
    `;
  }

  private renderButton(): unknown {
    const iconName = this.syncEnabled ? "icon-lock" : "icon-unlocked";
    const color: UUIInterfaceColor = this.syncEnabled ? "positive" : "danger";
    const label = this.localize.term(
      this.syncEnabled
        ? "nodenamesyncButton_labelUnlock"
        : "nodenamesyncButton_labelLock",
    );

    return html`<uui-button
      slot="append"
      @click=${this._toggleSync}
      .color=${color}
      label=${label}
      ><uui-icon .name=${iconName}></uui-icon
    ></uui-button>`;
  }

  static styles = [
    css`
      :host,
      uui-input {
        width: 100%;
      }
    `,
  ];
}

export { NodeNameSyncPropertyEditorUi as default };
