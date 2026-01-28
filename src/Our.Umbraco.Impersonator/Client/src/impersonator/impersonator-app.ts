import { LitElement, html } from "lit";
import { customElement } from "lit/decorators.js";
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { impersonate } from '../api';

@customElement('impersonator-app')
export class ImpersonatorApp extends UmbElementMixin(LitElement) {

	constructor() {
    super();
	}

  endImpersonation() {
    // todo
  }
  
  impersonateUser() {
	  impersonate({ query: { id: 'f6fb87cb-1fa2-469d-8e44-40d545c3b0e7' } });
  }

  setUserToImpersonate(event: Event) {
    if ((event.target as HTMLSelectElement).value) {
      this.userToImpersonate = (event.target as HTMLSelectElement).value;
    }
  }

  isImpersonating = false; // todo;
  users = []; // todo:
  userToImpersonate = '';

	render() {
    return this.isImpersonating ?
      html`
			<uui-box headline="Impersonator">
				<p>You are impersonating the current user</p>
				<uui-button look="primary" label="End impersonation" @click=${this.endImpersonation}></uui-button>
			</uui-box>
		` :
      html`
			<uui-box headline="Impersonator">
        <select class="umb-dropdown flx-g1 mb0" @change=${this.setUserToImpersonate}>
          <option>todo: get the users</option>
		  <option label="Tester" value="f6fb87cb-1fa2-469d-8e44-40d545c3b0e7">Tester</option>
        </select>
				<uui-button look="primary" label="Impersonate" @click=${this.impersonateUser}></uui-button>
			</uui-box>
      `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'impersonator-app': ImpersonatorApp;
	}
}

export default ImpersonatorApp;
