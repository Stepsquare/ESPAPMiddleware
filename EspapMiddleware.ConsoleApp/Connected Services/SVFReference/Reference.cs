﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EspapMiddleware.ConsoleApp.SVFReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="SVFReference.IService")]
    public interface IService {
        
        // CODEGEN: Generating message contract since the operation SendDocument is neither RPC nor document wrapped.
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IService/SendDocument")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        void SendDocument(EspapMiddleware.ConsoleApp.SVFReference.SendDocumentRequest request);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IService/SendDocument")]
        System.Threading.Tasks.Task SendDocumentAsync(EspapMiddleware.ConsoleApp.SVFReference.SendDocumentRequest request);
        
        // CODEGEN: Generating message contract since the operation SetDocumentResult is neither RPC nor document wrapped.
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IService/SetDocumentResult")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        void SetDocumentResult(EspapMiddleware.ConsoleApp.SVFReference.SetDocumentResultRequest request);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IService/SetDocumentResult")]
        System.Threading.Tasks.Task SetDocumentResultAsync(EspapMiddleware.ConsoleApp.SVFReference.SetDocumentResultRequest request);
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ElectronicInvoice.B2BClientOperations")]
    public partial class SendDocumentContract : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string uniqueIdField;
        
        private bool isAnUpdateField;
        
        private string documentIdField;
        
        private string referenceNumberField;
        
        private DocumentTypeEnum documentTypeField;
        
        private System.DateTime issueDateField;
        
        private string supplierFiscalIdField;
        
        private string customerFiscalIdField;
        
        private string internalManagementField;
        
        private string feapPortalUserField;
        
        private string ublFormatField;
        
        private string pdfFormatField;
        
        private string attachsField;
        
        private System.Nullable<DocumentStateEnum> stateIdField;
        
        private bool stateIdFieldSpecified;
        
        private System.Nullable<System.DateTime> stateDateField;
        
        private bool stateDateFieldSpecified;
        
        private System.Nullable<DocumentActionEnum> actionIdField;
        
        private bool actionIdFieldSpecified;
        
        private System.Nullable<System.DateTime> actionDateField;
        
        private bool actionDateFieldSpecified;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string uniqueId {
            get {
                return this.uniqueIdField;
            }
            set {
                this.uniqueIdField = value;
                this.RaisePropertyChanged("uniqueId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public bool isAnUpdate {
            get {
                return this.isAnUpdateField;
            }
            set {
                this.isAnUpdateField = value;
                this.RaisePropertyChanged("isAnUpdate");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=2)]
        public string documentId {
            get {
                return this.documentIdField;
            }
            set {
                this.documentIdField = value;
                this.RaisePropertyChanged("documentId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=3)]
        public string referenceNumber {
            get {
                return this.referenceNumberField;
            }
            set {
                this.referenceNumberField = value;
                this.RaisePropertyChanged("referenceNumber");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public DocumentTypeEnum documentType {
            get {
                return this.documentTypeField;
            }
            set {
                this.documentTypeField = value;
                this.RaisePropertyChanged("documentType");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=5)]
        public System.DateTime issueDate {
            get {
                return this.issueDateField;
            }
            set {
                this.issueDateField = value;
                this.RaisePropertyChanged("issueDate");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=6)]
        public string supplierFiscalId {
            get {
                return this.supplierFiscalIdField;
            }
            set {
                this.supplierFiscalIdField = value;
                this.RaisePropertyChanged("supplierFiscalId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=7)]
        public string customerFiscalId {
            get {
                return this.customerFiscalIdField;
            }
            set {
                this.customerFiscalIdField = value;
                this.RaisePropertyChanged("customerFiscalId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=8)]
        public string internalManagement {
            get {
                return this.internalManagementField;
            }
            set {
                this.internalManagementField = value;
                this.RaisePropertyChanged("internalManagement");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=9)]
        public string feapPortalUser {
            get {
                return this.feapPortalUserField;
            }
            set {
                this.feapPortalUserField = value;
                this.RaisePropertyChanged("feapPortalUser");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=10)]
        public string ublFormat {
            get {
                return this.ublFormatField;
            }
            set {
                this.ublFormatField = value;
                this.RaisePropertyChanged("ublFormat");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=11)]
        public string pdfFormat {
            get {
                return this.pdfFormatField;
            }
            set {
                this.pdfFormatField = value;
                this.RaisePropertyChanged("pdfFormat");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=12)]
        public string attachs {
            get {
                return this.attachsField;
            }
            set {
                this.attachsField = value;
                this.RaisePropertyChanged("attachs");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=13)]
        public System.Nullable<DocumentStateEnum> stateId {
            get {
                return this.stateIdField;
            }
            set {
                this.stateIdField = value;
                this.RaisePropertyChanged("stateId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stateIdSpecified {
            get {
                return this.stateIdFieldSpecified;
            }
            set {
                this.stateIdFieldSpecified = value;
                this.RaisePropertyChanged("stateIdSpecified");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=14)]
        public System.Nullable<System.DateTime> stateDate {
            get {
                return this.stateDateField;
            }
            set {
                this.stateDateField = value;
                this.RaisePropertyChanged("stateDate");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stateDateSpecified {
            get {
                return this.stateDateFieldSpecified;
            }
            set {
                this.stateDateFieldSpecified = value;
                this.RaisePropertyChanged("stateDateSpecified");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=15)]
        public System.Nullable<DocumentActionEnum> actionId {
            get {
                return this.actionIdField;
            }
            set {
                this.actionIdField = value;
                this.RaisePropertyChanged("actionId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actionIdSpecified {
            get {
                return this.actionIdFieldSpecified;
            }
            set {
                this.actionIdFieldSpecified = value;
                this.RaisePropertyChanged("actionIdSpecified");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=16)]
        public System.Nullable<System.DateTime> actionDate {
            get {
                return this.actionDateField;
            }
            set {
                this.actionDateField = value;
                this.RaisePropertyChanged("actionDate");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actionDateSpecified {
            get {
                return this.actionDateFieldSpecified;
            }
            set {
                this.actionDateFieldSpecified = value;
                this.RaisePropertyChanged("actionDateSpecified");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.datacontract.org/2004/07/EspapMiddleware.Shared.Enums")]
    public enum DocumentTypeEnum {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Item2,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Item3,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.datacontract.org/2004/07/EspapMiddleware.Shared.Enums")]
    public enum DocumentStateEnum {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("31")]
        Item31,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        Item7,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("35")]
        Item35,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        Item11,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("33")]
        Item33,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        Item22,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.datacontract.org/2004/07/EspapMiddleware.Shared.Enums")]
    public enum DocumentActionEnum {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        Item12,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("17")]
        Item17,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("19")]
        Item19,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("20")]
        Item20,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        Item22,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class SendDocumentRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ElectronicInvoice.B2BClientOperations", Order=0)]
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public EspapMiddleware.ConsoleApp.SVFReference.SendDocumentContract SendDocumentMCIn;
        
        public SendDocumentRequest() {
        }
        
        public SendDocumentRequest(EspapMiddleware.ConsoleApp.SVFReference.SendDocumentContract SendDocumentMCIn) {
            this.SendDocumentMCIn = SendDocumentMCIn;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:ElectronicInvoice.B2BClientOperations")]
    public partial class SetDocumentResultMCIn : object, System.ComponentModel.INotifyPropertyChanged {
        
        private System.Guid uniqueIdField;
        
        private string documentIdField;
        
        private string referenceNumberField;
        
        private System.Nullable<int> documentTypeField;
        
        private System.Nullable<System.DateTime> issueDateField;
        
        private string supplierFiscalIdField;
        
        private string customerFiscalIdField;
        
        private bool isASuccessField;
        
        private SetDocumentResultMCInMessages[] messagesField;
        
        private System.Nullable<int> stateIdField;
        
        private System.Nullable<int> actionIdField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public System.Guid uniqueId {
            get {
                return this.uniqueIdField;
            }
            set {
                this.uniqueIdField = value;
                this.RaisePropertyChanged("uniqueId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=1)]
        public string documentId {
            get {
                return this.documentIdField;
            }
            set {
                this.documentIdField = value;
                this.RaisePropertyChanged("documentId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=2)]
        public string referenceNumber {
            get {
                return this.referenceNumberField;
            }
            set {
                this.referenceNumberField = value;
                this.RaisePropertyChanged("referenceNumber");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=3)]
        public System.Nullable<int> documentType {
            get {
                return this.documentTypeField;
            }
            set {
                this.documentTypeField = value;
                this.RaisePropertyChanged("documentType");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=4)]
        public System.Nullable<System.DateTime> issueDate {
            get {
                return this.issueDateField;
            }
            set {
                this.issueDateField = value;
                this.RaisePropertyChanged("issueDate");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=5)]
        public string supplierFiscalId {
            get {
                return this.supplierFiscalIdField;
            }
            set {
                this.supplierFiscalIdField = value;
                this.RaisePropertyChanged("supplierFiscalId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=6)]
        public string customerFiscalId {
            get {
                return this.customerFiscalIdField;
            }
            set {
                this.customerFiscalIdField = value;
                this.RaisePropertyChanged("customerFiscalId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=7)]
        public bool isASuccess {
            get {
                return this.isASuccessField;
            }
            set {
                this.isASuccessField = value;
                this.RaisePropertyChanged("isASuccess");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("messages", IsNullable=true, Order=8)]
        public SetDocumentResultMCInMessages[] messages {
            get {
                return this.messagesField;
            }
            set {
                this.messagesField = value;
                this.RaisePropertyChanged("messages");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=9)]
        public System.Nullable<int> stateId {
            get {
                return this.stateIdField;
            }
            set {
                this.stateIdField = value;
                this.RaisePropertyChanged("stateId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=10)]
        public System.Nullable<int> actionId {
            get {
                return this.actionIdField;
            }
            set {
                this.actionIdField = value;
                this.RaisePropertyChanged("actionId");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:ElectronicInvoice.B2BClientOperations")]
    public partial class SetDocumentResultMCInMessages : object, System.ComponentModel.INotifyPropertyChanged {
        
        private int typeIdField;
        
        private string systemIdField;
        
        private string codeField;
        
        private string descriptionField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public int typeId {
            get {
                return this.typeIdField;
            }
            set {
                this.typeIdField = value;
                this.RaisePropertyChanged("typeId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=1)]
        public string systemId {
            get {
                return this.systemIdField;
            }
            set {
                this.systemIdField = value;
                this.RaisePropertyChanged("systemId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=2)]
        public string code {
            get {
                return this.codeField;
            }
            set {
                this.codeField = value;
                this.RaisePropertyChanged("code");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=3)]
        public string description {
            get {
                return this.descriptionField;
            }
            set {
                this.descriptionField = value;
                this.RaisePropertyChanged("description");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class SetDocumentResultRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ElectronicInvoice.B2BClientOperations", Order=0)]
        public EspapMiddleware.ConsoleApp.SVFReference.SetDocumentResultMCIn SetDocumentResultMCIn;
        
        public SetDocumentResultRequest() {
        }
        
        public SetDocumentResultRequest(EspapMiddleware.ConsoleApp.SVFReference.SetDocumentResultMCIn SetDocumentResultMCIn) {
            this.SetDocumentResultMCIn = SetDocumentResultMCIn;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IServiceChannel : EspapMiddleware.ConsoleApp.SVFReference.IService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ServiceClient : System.ServiceModel.ClientBase<EspapMiddleware.ConsoleApp.SVFReference.IService>, EspapMiddleware.ConsoleApp.SVFReference.IService {
        
        public ServiceClient() {
        }
        
        public ServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        void EspapMiddleware.ConsoleApp.SVFReference.IService.SendDocument(EspapMiddleware.ConsoleApp.SVFReference.SendDocumentRequest request) {
            base.Channel.SendDocument(request);
        }
        
        public void SendDocument(EspapMiddleware.ConsoleApp.SVFReference.SendDocumentContract SendDocumentMCIn) {
            EspapMiddleware.ConsoleApp.SVFReference.SendDocumentRequest inValue = new EspapMiddleware.ConsoleApp.SVFReference.SendDocumentRequest();
            inValue.SendDocumentMCIn = SendDocumentMCIn;
            ((EspapMiddleware.ConsoleApp.SVFReference.IService)(this)).SendDocument(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task EspapMiddleware.ConsoleApp.SVFReference.IService.SendDocumentAsync(EspapMiddleware.ConsoleApp.SVFReference.SendDocumentRequest request) {
            return base.Channel.SendDocumentAsync(request);
        }
        
        public System.Threading.Tasks.Task SendDocumentAsync(EspapMiddleware.ConsoleApp.SVFReference.SendDocumentContract SendDocumentMCIn) {
            EspapMiddleware.ConsoleApp.SVFReference.SendDocumentRequest inValue = new EspapMiddleware.ConsoleApp.SVFReference.SendDocumentRequest();
            inValue.SendDocumentMCIn = SendDocumentMCIn;
            return ((EspapMiddleware.ConsoleApp.SVFReference.IService)(this)).SendDocumentAsync(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        void EspapMiddleware.ConsoleApp.SVFReference.IService.SetDocumentResult(EspapMiddleware.ConsoleApp.SVFReference.SetDocumentResultRequest request) {
            base.Channel.SetDocumentResult(request);
        }
        
        public void SetDocumentResult(EspapMiddleware.ConsoleApp.SVFReference.SetDocumentResultMCIn SetDocumentResultMCIn) {
            EspapMiddleware.ConsoleApp.SVFReference.SetDocumentResultRequest inValue = new EspapMiddleware.ConsoleApp.SVFReference.SetDocumentResultRequest();
            inValue.SetDocumentResultMCIn = SetDocumentResultMCIn;
            ((EspapMiddleware.ConsoleApp.SVFReference.IService)(this)).SetDocumentResult(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task EspapMiddleware.ConsoleApp.SVFReference.IService.SetDocumentResultAsync(EspapMiddleware.ConsoleApp.SVFReference.SetDocumentResultRequest request) {
            return base.Channel.SetDocumentResultAsync(request);
        }
        
        public System.Threading.Tasks.Task SetDocumentResultAsync(EspapMiddleware.ConsoleApp.SVFReference.SetDocumentResultMCIn SetDocumentResultMCIn) {
            EspapMiddleware.ConsoleApp.SVFReference.SetDocumentResultRequest inValue = new EspapMiddleware.ConsoleApp.SVFReference.SetDocumentResultRequest();
            inValue.SetDocumentResultMCIn = SetDocumentResultMCIn;
            return ((EspapMiddleware.ConsoleApp.SVFReference.IService)(this)).SetDocumentResultAsync(inValue);
        }
    }
}