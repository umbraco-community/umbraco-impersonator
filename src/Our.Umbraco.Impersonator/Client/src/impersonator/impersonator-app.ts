import { LitElement, html } from "lit";
import { customElement } from "lit/decorators.js";
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { impersonate, endImpersonation } from '../api';

@customElement('impersonator-app')
export class ImpersonatorApp extends UmbElementMixin(LitElement) {

    constructor() {
        super();
    }

    triggerLoginAuthFlow() {
        // This stores the auth of the current user so get shot
        localStorage.removeItem('umb:userAuthTokenResponse');

        // And as we expect the call to sign the user in has already happened,
        // we boot the user back to the root of the app which will retrigger
        // the front-end (back office) authentication flow
        window.location.href = "/umbraco/";
    }

    endImpersonationClick() {

        let t = this;

        endImpersonation().then(function (response) {
            if (response.data == "success") {
                // Clear cached OAuth tokens so the frontend requests new ones for the impersonated user
                t.triggerLoginAuthFlow();
            } else {
                /*vm.impersonateButtonState = "error";
                localizationService
                    .localize("impersonator_" + response.data)
                    .then(function (value) {
                        notificationsService.error(value);
                    });*/
                alert("ERROR with impersonation " + response.data);
            }
        })
    }

    impersonateUserClick() {

        let t = this;

        impersonate({ query: { id: 'f6fb87cb-1fa2-469d-8e44-40d545c3b0e7' } }).then(function (response) {
            if (response.data == "success") {
                // Clear cached OAuth tokens so the frontend requests new ones for the impersonated user
                t.triggerLoginAuthFlow();
            } else {
                /*vm.impersonateButtonState = "error";
                localizationService
                    .localize("impersonator_" + response.data)
                    .then(function (value) {
                        notificationsService.error(value);
                    });*/
                alert("ERROR with impersonation " + response.data);
            }
        });
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
        //this.isImpersonating ?
        return html`
			<uui-box headline="Impersonator">
				<p>You are impersonating the current user</p>
				<uui-button look="primary" label="End impersonation" @click=${this.endImpersonationClick}></uui-button>
			</uui-box>

            <!-- CHECK IF IMPERSONATING!! -->


			<uui-box headline="Impersonator">
        <select class="umb-dropdown flx-g1 mb0" @change=${this.setUserToImpersonate}>
          <option>todo: get the users</option>
		  <option label="Tester" value="f6fb87cb-1fa2-469d-8e44-40d545c3b0e7">Tester</option>
        </select>
				<uui-button look="primary" label="Impersonate" @click=${this.impersonateUserClick}></uui-button>
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
