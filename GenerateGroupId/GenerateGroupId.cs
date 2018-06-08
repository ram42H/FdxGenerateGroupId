using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace GenerateGroupId
{
    public sealed partial class GenerateGroupId : CodeActivity
    {
        //[Input("ParentRecordId")]
        //[ReferenceTarget("fdx_parentleadid")]
        //public InArgument<EntityReference> InputEntity { get; set; }

        [Output("Group Id")]
        public OutArgument<string> OutputEntity { get; set; }

        [Output("Parent Account")]
        [ReferenceTarget("account")]
        public OutArgument<EntityReference> parentAccount { get; set; } 

        protected override void Execute(CodeActivityContext context)
        {
            //Extract the tracing service for use in debugging sandboxed plug-ins....
            ITracingService tracingService = context.GetExtension<ITracingService>(); 

            int step = 0;
            try
            {
                IWorkflowContext WorkflowContext = context.GetExtension<IWorkflowContext>();
                IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
                IOrganizationService service = serviceFactory.CreateOrganizationService(WorkflowContext.UserId);

                step = 1;

                Entity LeadEntity = service.Retrieve("lead", WorkflowContext.PrimaryEntityId, new ColumnSet("fdx_leadid", "parentaccountid"));

                step = 2;
                if (LeadEntity.Attributes.Contains("fdx_leadid"))
                {
                    step = 3;
                    Entity ParentLeadEntity = service.Retrieve("lead", ((EntityReference)LeadEntity.Attributes["fdx_leadid"]).Id, new ColumnSet("fdx_groupid", "leadid", "fdx_leadid", "parentaccountid"));

                    step = 4;
                    if (ParentLeadEntity.Attributes.Contains("fdx_groupid"))
                    {
                        step = 5;
                        this.OutputEntity.Set(context, ParentLeadEntity.Attributes["fdx_groupid"]);
                        step = 6;
                    }
                    else
                    {
                        step = 7;
                        this.OutputEntity.Set(context, null);
                        step = 8;
                    }

                    #region SMART-821: Tag account to connected lead with context lead's account if empty
                    if (!LeadEntity.Attributes.Contains("parentaccountid"))
                    {
                        if(ParentLeadEntity.Attributes.Contains("parentaccountid"))
                        {
                            this.parentAccount.Set(context, (EntityReference)ParentLeadEntity.Attributes["parentaccountid"]);

                            tracingService.Trace("account Guid -" + this.parentAccount);
                        }
                    }
                    else
                    {
                        this.parentAccount.Set(context, (EntityReference)LeadEntity.Attributes["parentaccountid"]);

                        tracingService.Trace("account Guid -" + this.parentAccount);
                    }
                    #endregion                    
                }
                else
                {
                    step = 9;
                    this.OutputEntity.Set(context, WorkflowContext.PrimaryEntityId.ToString());

                    #region SMART-821: Tag account to connected lead with context lead's account if empty
                    if (LeadEntity.Attributes.Contains("parentaccountid"))
                    {
                        this.parentAccount.Set(context, (EntityReference)LeadEntity.Attributes["parentaccountid"]);

                        tracingService.Trace("account Guid -" + this.parentAccount);
                    }
                    #endregion
                }
            }
            catch(Exception ex)
            {
                throw new NotImplementedException(string.Format("Error while Generating Group Number,at Step = {0}. Message: {1} ",step, ex.Message));
            }
        }
    }
}
