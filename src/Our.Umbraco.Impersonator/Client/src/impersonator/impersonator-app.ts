import { LitElement, html } from "lit";
import { customElement } from "lit/decorators.js";
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { impersonate, endImpersonation, getUsers, getImpersonatingUserName } from '../api';
import { css, state } from "@umbraco-cms/backoffice/external/lit";

@customElement('impersonator-app')
export class ImpersonatorApp extends UmbElementMixin(LitElement) {

    static styles = css`
        .ddl-wrap {
            display:flex;
            gap:5px;
        }
        .flx-g1 {
            flex-grow:1;
        }
        .mt-0 { margin-top:0; }
    `;

    // The user list we can choose from to impersonate
    @state()
    private userOptions: Array<Option> = [];

    // A flag to say if we're impersonating or not
    @state()
    private isImpersonating = false;

    // The name of the user we're impersonating, if we are
    @state()
    private impersonatingUserName = '';

    @state()
    private isLoading = true;

    @state()
    private isError = false;

    // A flag controlling whether this element should display or not
    @state()
    private show = false;

    userToImpersonate = '';

    constructor() {
        super();
    }

    connectedCallback() {
        super.connectedCallback();

        let t = this;

        // Do some loading logic here to fetch the necessary data
        getImpersonatingUserName().then(function (data) {
            // If we get something back here we have a user that we are impersonating...
            if (data.data) {
                t.isImpersonating = true;
                t.impersonatingUserName = data.data;
                t.show = true;
                t.isLoading = false;

            } else {
                // Otherwise we could be in a position to impersonate...
                getUsers().then(function (data) {
                    console.log("got some data", data);
                    t.isLoading = false;
                    if (!data.response.ok) {
                        // Here we don't have permissions (or internal error)
                        // so don't set t.show = true; - effectively hiding
                        // the package from view
                        t.isError = true;
                    }
                    else {

                        // We are NOT impersonating, and we have data! Let's rock
                        t.userOptions = data.data?.map(ele => {
                            return {
                                value: ele.key,
                                name: ele.name ?? 'Unknown'
                            };
                        }) ?? [];

                        t.show = true;
                    }
                });
            }
        })
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
                alert("ERROR with impersonation " + response.data);
            }
        })
    }

    impersonateUserClick() {

        let t = this;

        impersonate({ query: { id: this.userToImpersonate } }).then(function (response) {
            if (response.data == "success") {
                // Clear cached OAuth tokens so the frontend requests new ones for the impersonated user
                t.triggerLoginAuthFlow();
            } else {
                alert("ERROR with impersonation " + response.data);
            }
        });
    }

    setUserToImpersonate(event: Event) {
        if ((event.target as HTMLSelectElement).value) {
            this.userToImpersonate = (event.target as HTMLSelectElement).value;
        }
    }

    render() {
        // If we haven't set this to view, then hide the box altogether
        if (!this.show) { return html``; }

        return html`
			<uui-box headline="Impersonator">

                ${this.isLoading ? html`` : html`${this.isError ? html`You do not have permissions to impersonate` : html`${this.getLayout()}` }` }
                
			</uui-box>
      `;
    }

    getLayout() {
        return html`${this.isImpersonating ?

            html`<p class="mt-0">You are impersonating <strong>${this.impersonatingUserName}</strong></p>
				    <uui-button look="primary" label="End impersonation" @click=${this.endImpersonationClick}></uui-button>`

            :

            html`
                    <div class="ddl-wrap">
                        ${this.getUserDropdownHtml()}
				        <uui-button look="primary" label="Impersonate" @click=${this.impersonateUserClick}></uui-button>
                    </div>
                    `
            }`;
    }

    // Generate the markup for the user selection drop down
    getUserDropdownHtml() {
        if (!this.userOptions || this.userOptions.length === 0) return html``;
        return html`<uui-select class="umb-dropdown flx-g1 mb0" @change=${this.setUserToImpersonate} .options=${this.userOptions} .placeholder=${"Select a user"}>
    </uui-select>`;
    }
}

declare global {
    interface HTMLElementTagNameMap {
        'impersonator-app': ImpersonatorApp;
    }
}

export default ImpersonatorApp;
