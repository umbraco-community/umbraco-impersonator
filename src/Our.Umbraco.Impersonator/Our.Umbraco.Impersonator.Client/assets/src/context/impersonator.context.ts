import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { ImpersonatorSource } from "../repository/sources/impersonator.source";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { ImpersonationResultEnum, OpenAPI } from './../api/index.ts';
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UMB_NOTIFICATION_CONTEXT, UmbNotificationContext } from "@umbraco-cms/backoffice/notification";

export default class ImpersonatorContext extends UmbControllerBase {

    #source: ImpersonatorSource;
    #notificationContext?: UmbNotificationContext;

    constructor(host: UmbControllerHost) {
        super(host);

        this.provideContext(IMPERSONATOR_CONTEXT_TOKEN, this);
        this.#source = new ImpersonatorSource(this);

        this.consumeContext(UMB_NOTIFICATION_CONTEXT, (_notification) => {
            this.#notificationContext = _notification;
        });

        this.consumeContext(UMB_AUTH_CONTEXT, (_auth) => {
            const umbOpenApi = _auth.getOpenApiConfiguration();
            OpenAPI.TOKEN = umbOpenApi.token;
            OpenAPI.BASE = umbOpenApi.base;
            OpenAPI.WITH_CREDENTIALS = umbOpenApi.withCredentials;
        });
    }

    async getImpersonatingUser() {
        return (await this.#source.getImpersonatingUser()).data;
    }

    async endImpersonation() {
        const result = await this.#source.endImpersonation();
        switch (result.data) {
            case ImpersonationResultEnum.NOT_SIGNED_IN:
                this.#notificationContext?.peek('danger', {
                    data: {
                        message: 'You are not signed in'
                    }
                });
                break;
            case ImpersonationResultEnum.SUCCESS:
                this.#notificationContext?.peek('positive', {
                    data: {
                        message: 'Impersonation ended, reloading backoffice...'
                    }
                });
                window.location.reload();
                break;
            case ImpersonationResultEnum.USER_NOT_FOUND:
                this.#notificationContext?.peek('danger', {
                    data: {
                        message: 'End impersonation failed, user not found'
                    }
                });
                break;
            case ImpersonationResultEnum.ACCESS_DENIED:
                this.#notificationContext?.peek('danger', {
                    data: {
                        message: 'End impersonation failed, you are not authorized to perform this action'
                    }
                });
                break;
            default:
                this.#notificationContext?.peek('danger', {
                    data: {
                        message: 'End impersonation failed for unknown reasons'
                    }
                });
                break;
        }
    }

    async impersonate(id: string) {
        const result = await this.#source.impersonate(id);
        switch (result.data) {
            case ImpersonationResultEnum.SUCCESS:
                this.#notificationContext?.peek('positive', {
                    data: {
                        message: 'Impersonation successful, reloading backoffice...'
                    }
                });
                window.location.reload();
                break;
            case ImpersonationResultEnum.USER_NOT_FOUND:
                this.#notificationContext?.peek('danger', {
                    data: {
                        message: 'Impersonation failed, user not found'
                    }
                });
                break;
            case ImpersonationResultEnum.ACCESS_DENIED:
                this.#notificationContext?.peek('danger', {
                    data: {
                        message: 'Impersonation failed, you are not authorized to perform this action'
                    }
                });
                break;
            default:
                this.#notificationContext?.peek('danger', {
                    data: {
                        message: 'Impersonation failed for unknown reasons'
                    }
                });
                break;
        }
    }
}

export const IMPERSONATOR_CONTEXT_TOKEN =
    new UmbContextToken<ImpersonatorContext>(ImpersonatorContext.name);