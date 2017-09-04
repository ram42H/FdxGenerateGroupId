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
        protected override void Execute(CodeActivityContext context)
        {
            int step = 0;
            try
            {
                IWorkflowContext WorkflowContext = context.GetExtension<IWorkflowContext>();
                IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
                IOrganizationService service = serviceFactory.CreateOrganizationService(WorkflowContext.UserId);

                step = 1;

                Entity LeadEntity = service.Retrieve("lead", WorkflowContext.PrimaryEntityId, new ColumnSet("fdx_leadid"));

                step = 2;
                if (LeadEntity.Attributes.Contains("fdx_leadid"))
                {
                    step = 3;
                    Entity ParentLeadEntity = service.Retrieve("lead", ((EntityReference)LeadEntity.Attributes["fdx_leadid"]).Id, new ColumnSet("fdx_groupid", "leadid", "fdx_leadid"));
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
                    //Commented because this case should be checked by Sales Rep, and run an On Demand WF on both Parent and Child
                    /*else
                    {
                        step = 7;
                        this.OutputEntity.Set(context, WorkflowContext.PrimaryEntityId.ToString());//((EntityReference)LeadEntity.Attributes["leadid"]).Id.ToString());
                        step = 8;
                    }*/
                }
                else
                {
                    step = 9;
                    this.OutputEntity.Set(context, WorkflowContext.PrimaryEntityId.ToString());
                }
                //Entity counter = service.Retrieve("fdx_snbcounter", this.InputEntity.Get(executionContext).Id, new ColumnSet("fdx_currentcounter"));
            }
            catch(Exception ex)
            {
                throw new NotImplementedException(string.Format("Error while Generating Group Number,at Step = {0}. Message: {1} ",step, ex.Message));
            }
        }
    }
}
