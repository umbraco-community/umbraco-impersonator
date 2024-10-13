import { UmbControllerHostElement } from "@umbraco-cms/backoffice/controller-api";
import { UmbEntityActionBase, UmbEntityActionArgs } from "@umbraco-cms/backoffice/entity-action";
import { IMPERSONATOR_CONTEXT_TOKEN } from "../../context/impersonator.context";
import { UMB_NOTIFICATION_CONTEXT, UmbNotificationContext } from "@umbraco-cms/backoffice/notification";

export default class ImpersonateAction extends UmbEntityActionBase<never> {

    #impersonatorContext?: typeof IMPERSONATOR_CONTEXT_TOKEN.TYPE;
    #notificationContext?: UmbNotificationContext;

    constructor(host: UmbControllerHostElement, args: UmbEntityActionArgs<never>) {
        super(host, args);

        this.consumeContext(IMPERSONATOR_CONTEXT_TOKEN, (context) => {
            this.#impersonatorContext = context;
        });

        this.consumeContext(UMB_NOTIFICATION_CONTEXT, (_notification) => {
            this.#notificationContext = _notification;
        });
    }

    async execute() {

        if (!this.args.unique) {
            this.#notificationContext?.peek('danger', {
                data: {
                    message: 'No user id provided'
                }
            });
            return;
        }

        await this.#impersonatorContext?.impersonate(this.args.unique?.toString());
    }
}