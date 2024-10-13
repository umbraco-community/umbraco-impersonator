import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { ImpersonatorApiService } from "../../api";

export class ImpersonatorSource {

    #host: UmbControllerHost;

    constructor(host: UmbControllerHost) {
        this.#host = host;
    }

    async getImpersonatingUser() {
        return await tryExecuteAndNotify(this.#host, ImpersonatorApiService.getUmbracoManagementApiV1ImpersonatorApiUser());
    }

    async endImpersonation() {
        return await tryExecuteAndNotify(this.#host, ImpersonatorApiService.deleteUmbracoManagementApiV1ImpersonatorApiUser());
    }

    async impersonate(id: string) {
        return await tryExecuteAndNotify(this.#host, ImpersonatorApiService.putUmbracoManagementApiV1ImpersonatorApiUser({
            id
        }));
    }
}