﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EspapMiddleware.ConsoleApp.LocalSVFReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SendDocumentContract", Namespace="urnl:ElectronicInvoice.B2BClientOperations")]
    [System.SerializableAttribute()]
    public partial class SendDocumentContract : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Guid uniqueIdField;
        
        private bool isAnUpdateField;
        
        private string documentIdField;
        
        private string referenceNumberField;
        
        private EspapMiddleware.ConsoleApp.LocalSVFReference.DocumentTypeEnum documentTypeField;
        
        private System.DateTime issueDateField;
        
        private string supplierFiscalIdField;
        
        private string customerFiscalIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string internalManagementField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string feapPortalUserField;
        
        private string ublFormatField;
        
        private string pdfFormatField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string attachsField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<EspapMiddleware.ConsoleApp.LocalSVFReference.DocumentStateEnum> stateIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<System.DateTime> stateDateField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<EspapMiddleware.ConsoleApp.LocalSVFReference.DocumentActionEnum> actionIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<System.DateTime> actionDateField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public System.Guid uniqueId {
            get {
                return this.uniqueIdField;
            }
            set {
                if ((this.uniqueIdField.Equals(value) != true)) {
                    this.uniqueIdField = value;
                    this.RaisePropertyChanged("uniqueId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=1)]
        public bool isAnUpdate {
            get {
                return this.isAnUpdateField;
            }
            set {
                if ((this.isAnUpdateField.Equals(value) != true)) {
                    this.isAnUpdateField = value;
                    this.RaisePropertyChanged("isAnUpdate");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=2)]
        public string documentId {
            get {
                return this.documentIdField;
            }
            set {
                if ((object.ReferenceEquals(this.documentIdField, value) != true)) {
                    this.documentIdField = value;
                    this.RaisePropertyChanged("documentId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=3)]
        public string referenceNumber {
            get {
                return this.referenceNumberField;
            }
            set {
                if ((object.ReferenceEquals(this.referenceNumberField, value) != true)) {
                    this.referenceNumberField = value;
                    this.RaisePropertyChanged("referenceNumber");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=4)]
        public EspapMiddleware.ConsoleApp.LocalSVFReference.DocumentTypeEnum documentType {
            get {
                return this.documentTypeField;
            }
            set {
                if ((this.documentTypeField.Equals(value) != true)) {
                    this.documentTypeField = value;
                    this.RaisePropertyChanged("documentType");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=5)]
        public System.DateTime issueDate {
            get {
                return this.issueDateField;
            }
            set {
                if ((this.issueDateField.Equals(value) != true)) {
                    this.issueDateField = value;
                    this.RaisePropertyChanged("issueDate");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=6)]
        public string supplierFiscalId {
            get {
                return this.supplierFiscalIdField;
            }
            set {
                if ((object.ReferenceEquals(this.supplierFiscalIdField, value) != true)) {
                    this.supplierFiscalIdField = value;
                    this.RaisePropertyChanged("supplierFiscalId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=7)]
        public string customerFiscalId {
            get {
                return this.customerFiscalIdField;
            }
            set {
                if ((object.ReferenceEquals(this.customerFiscalIdField, value) != true)) {
                    this.customerFiscalIdField = value;
                    this.RaisePropertyChanged("customerFiscalId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=8)]
        public string internalManagement {
            get {
                return this.internalManagementField;
            }
            set {
                if ((object.ReferenceEquals(this.internalManagementField, value) != true)) {
                    this.internalManagementField = value;
                    this.RaisePropertyChanged("internalManagement");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=9)]
        public string feapPortalUser {
            get {
                return this.feapPortalUserField;
            }
            set {
                if ((object.ReferenceEquals(this.feapPortalUserField, value) != true)) {
                    this.feapPortalUserField = value;
                    this.RaisePropertyChanged("feapPortalUser");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=10)]
        public string ublFormat {
            get {
                return this.ublFormatField;
            }
            set {
                if ((object.ReferenceEquals(this.ublFormatField, value) != true)) {
                    this.ublFormatField = value;
                    this.RaisePropertyChanged("ublFormat");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=11)]
        public string pdfFormat {
            get {
                return this.pdfFormatField;
            }
            set {
                if ((object.ReferenceEquals(this.pdfFormatField, value) != true)) {
                    this.pdfFormatField = value;
                    this.RaisePropertyChanged("pdfFormat");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=12)]
        public string attachs {
            get {
                return this.attachsField;
            }
            set {
                if ((object.ReferenceEquals(this.attachsField, value) != true)) {
                    this.attachsField = value;
                    this.RaisePropertyChanged("attachs");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=13)]
        public System.Nullable<EspapMiddleware.ConsoleApp.LocalSVFReference.DocumentStateEnum> stateId {
            get {
                return this.stateIdField;
            }
            set {
                if ((this.stateIdField.Equals(value) != true)) {
                    this.stateIdField = value;
                    this.RaisePropertyChanged("stateId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=14)]
        public System.Nullable<System.DateTime> stateDate {
            get {
                return this.stateDateField;
            }
            set {
                if ((this.stateDateField.Equals(value) != true)) {
                    this.stateDateField = value;
                    this.RaisePropertyChanged("stateDate");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=15)]
        public System.Nullable<EspapMiddleware.ConsoleApp.LocalSVFReference.DocumentActionEnum> actionId {
            get {
                return this.actionIdField;
            }
            set {
                if ((this.actionIdField.Equals(value) != true)) {
                    this.actionIdField = value;
                    this.RaisePropertyChanged("actionId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=16)]
        public System.Nullable<System.DateTime> actionDate {
            get {
                return this.actionDateField;
            }
            set {
                if ((this.actionDateField.Equals(value) != true)) {
                    this.actionDateField = value;
                    this.RaisePropertyChanged("actionDate");
                }
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
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="DocumentTypeEnum", Namespace="http://schemas.datacontract.org/2004/07/EspapMiddleware.Shared.Enums")]
    public enum DocumentTypeEnum : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="1")]
        _1 = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="2")]
        _2 = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="3")]
        _3 = 3,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="DocumentStateEnum", Namespace="http://schemas.datacontract.org/2004/07/EspapMiddleware.Shared.Enums")]
    public enum DocumentStateEnum : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="1")]
        _1 = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="31")]
        _31 = 31,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="7")]
        _7 = 7,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="35")]
        _35 = 35,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="11")]
        _11 = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="33")]
        _33 = 33,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="22")]
        _22 = 22,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="DocumentActionEnum", Namespace="http://schemas.datacontract.org/2004/07/EspapMiddleware.Shared.Enums")]
    public enum DocumentActionEnum : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="12")]
        _12 = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="17")]
        _17 = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="19")]
        _19 = 19,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="20")]
        _20 = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="22")]
        _22 = 22,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SetDocumentResultContract", Namespace="urn:ElectronicInvoice.B2BClientOperations")]
    [System.SerializableAttribute()]
    public partial class SetDocumentResultContract : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private System.Guid uniqueIdField;
        
        private string documentIdField;
        
        private string referenceNumberField;
        
        private EspapMiddleware.ConsoleApp.LocalSVFReference.DocumentTypeEnum documentTypeField;
        
        private System.DateTime issueDateField;
        
        private string supplierFiscalIdField;
        
        private string customerFiscalIdField;
        
        private bool isASuccessField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultContract.Message[] messagesField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<EspapMiddleware.ConsoleApp.LocalSVFReference.DocumentStateEnum> stateIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<EspapMiddleware.ConsoleApp.LocalSVFReference.DocumentActionEnum> actionIdField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public System.Guid uniqueId {
            get {
                return this.uniqueIdField;
            }
            set {
                if ((this.uniqueIdField.Equals(value) != true)) {
                    this.uniqueIdField = value;
                    this.RaisePropertyChanged("uniqueId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=1)]
        public string documentId {
            get {
                return this.documentIdField;
            }
            set {
                if ((object.ReferenceEquals(this.documentIdField, value) != true)) {
                    this.documentIdField = value;
                    this.RaisePropertyChanged("documentId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=2)]
        public string referenceNumber {
            get {
                return this.referenceNumberField;
            }
            set {
                if ((object.ReferenceEquals(this.referenceNumberField, value) != true)) {
                    this.referenceNumberField = value;
                    this.RaisePropertyChanged("referenceNumber");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=3)]
        public EspapMiddleware.ConsoleApp.LocalSVFReference.DocumentTypeEnum documentType {
            get {
                return this.documentTypeField;
            }
            set {
                if ((this.documentTypeField.Equals(value) != true)) {
                    this.documentTypeField = value;
                    this.RaisePropertyChanged("documentType");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=4)]
        public System.DateTime issueDate {
            get {
                return this.issueDateField;
            }
            set {
                if ((this.issueDateField.Equals(value) != true)) {
                    this.issueDateField = value;
                    this.RaisePropertyChanged("issueDate");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=5)]
        public string supplierFiscalId {
            get {
                return this.supplierFiscalIdField;
            }
            set {
                if ((object.ReferenceEquals(this.supplierFiscalIdField, value) != true)) {
                    this.supplierFiscalIdField = value;
                    this.RaisePropertyChanged("supplierFiscalId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=6)]
        public string customerFiscalId {
            get {
                return this.customerFiscalIdField;
            }
            set {
                if ((object.ReferenceEquals(this.customerFiscalIdField, value) != true)) {
                    this.customerFiscalIdField = value;
                    this.RaisePropertyChanged("customerFiscalId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=7)]
        public bool isASuccess {
            get {
                return this.isASuccessField;
            }
            set {
                if ((this.isASuccessField.Equals(value) != true)) {
                    this.isASuccessField = value;
                    this.RaisePropertyChanged("isASuccess");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=8)]
        public EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultContract.Message[] messages {
            get {
                return this.messagesField;
            }
            set {
                if ((object.ReferenceEquals(this.messagesField, value) != true)) {
                    this.messagesField = value;
                    this.RaisePropertyChanged("messages");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=9)]
        public System.Nullable<EspapMiddleware.ConsoleApp.LocalSVFReference.DocumentStateEnum> stateId {
            get {
                return this.stateIdField;
            }
            set {
                if ((this.stateIdField.Equals(value) != true)) {
                    this.stateIdField = value;
                    this.RaisePropertyChanged("stateId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=10)]
        public System.Nullable<EspapMiddleware.ConsoleApp.LocalSVFReference.DocumentActionEnum> actionId {
            get {
                return this.actionIdField;
            }
            set {
                if ((this.actionIdField.Equals(value) != true)) {
                    this.actionIdField = value;
                    this.RaisePropertyChanged("actionId");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
        
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
        [System.Runtime.Serialization.DataContractAttribute(Name="SetDocumentResultContract.Message", Namespace="urn:ElectronicInvoice.B2BClientOperations")]
        [System.SerializableAttribute()]
        public partial class Message : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
            
            [System.NonSerializedAttribute()]
            private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
            
            private EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultContractMessageMessageType typeIdField;
            
            [System.Runtime.Serialization.OptionalFieldAttribute()]
            private string systemIdField;
            
            [System.Runtime.Serialization.OptionalFieldAttribute()]
            private string codeField;
            
            private string descriptionField;
            
            public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
                get {
                    return this.extensionDataField;
                }
                set {
                    this.extensionDataField = value;
                }
            }
            
            [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
            public EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultContractMessageMessageType typeId {
                get {
                    return this.typeIdField;
                }
                set {
                    if ((this.typeIdField.Equals(value) != true)) {
                        this.typeIdField = value;
                        this.RaisePropertyChanged("typeId");
                    }
                }
            }
            
            [System.Runtime.Serialization.DataMemberAttribute(Order=1)]
            public string systemId {
                get {
                    return this.systemIdField;
                }
                set {
                    if ((object.ReferenceEquals(this.systemIdField, value) != true)) {
                        this.systemIdField = value;
                        this.RaisePropertyChanged("systemId");
                    }
                }
            }
            
            [System.Runtime.Serialization.DataMemberAttribute(Order=2)]
            public string code {
                get {
                    return this.codeField;
                }
                set {
                    if ((object.ReferenceEquals(this.codeField, value) != true)) {
                        this.codeField = value;
                        this.RaisePropertyChanged("code");
                    }
                }
            }
            
            [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=3)]
            public string description {
                get {
                    return this.descriptionField;
                }
                set {
                    if ((object.ReferenceEquals(this.descriptionField, value) != true)) {
                        this.descriptionField = value;
                        this.RaisePropertyChanged("description");
                    }
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
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SetDocumentResultContract.Message.MessageType", Namespace="http://schemas.datacontract.org/2004/07/EspapMiddleware.Shared.DataContracts")]
    public enum SetDocumentResultContractMessageMessageType : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="1")]
        _1 = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="2")]
        _2 = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute(Value="3")]
        _3 = 3,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="LocalSVFReference.IService")]
    public interface IService {
        
        // CODEGEN: Generating message contract since the operation SendDocument is neither RPC nor document wrapped.
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IService/SendDocument")]
        void SendDocument(EspapMiddleware.ConsoleApp.LocalSVFReference.SendDocumentRequest request);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IService/SendDocument")]
        System.Threading.Tasks.Task SendDocumentAsync(EspapMiddleware.ConsoleApp.LocalSVFReference.SendDocumentRequest request);
        
        // CODEGEN: Generating message contract since the operation SetDocumentResult is neither RPC nor document wrapped.
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IService/SetDocumentResult")]
        void SetDocumentResult(EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultRequest request);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IService/SetDocumentResult")]
        System.Threading.Tasks.Task SetDocumentResultAsync(EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class SendDocumentRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urnl:ElectronicInvoice.B2BClientOperations", Order=0)]
        public EspapMiddleware.ConsoleApp.LocalSVFReference.SendDocumentContract SendDocumentMCIn;
        
        public SendDocumentRequest() {
        }
        
        public SendDocumentRequest(EspapMiddleware.ConsoleApp.LocalSVFReference.SendDocumentContract SendDocumentMCIn) {
            this.SendDocumentMCIn = SendDocumentMCIn;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class SetDocumentResultRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultContract SetDocumentResultMCIn;
        
        public SetDocumentResultRequest() {
        }
        
        public SetDocumentResultRequest(EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultContract SetDocumentResultMCIn) {
            this.SetDocumentResultMCIn = SetDocumentResultMCIn;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IServiceChannel : EspapMiddleware.ConsoleApp.LocalSVFReference.IService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ServiceClient : System.ServiceModel.ClientBase<EspapMiddleware.ConsoleApp.LocalSVFReference.IService>, EspapMiddleware.ConsoleApp.LocalSVFReference.IService {
        
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
        void EspapMiddleware.ConsoleApp.LocalSVFReference.IService.SendDocument(EspapMiddleware.ConsoleApp.LocalSVFReference.SendDocumentRequest request) {
            base.Channel.SendDocument(request);
        }
        
        public void SendDocument(EspapMiddleware.ConsoleApp.LocalSVFReference.SendDocumentContract SendDocumentMCIn) {
            EspapMiddleware.ConsoleApp.LocalSVFReference.SendDocumentRequest inValue = new EspapMiddleware.ConsoleApp.LocalSVFReference.SendDocumentRequest();
            inValue.SendDocumentMCIn = SendDocumentMCIn;
            ((EspapMiddleware.ConsoleApp.LocalSVFReference.IService)(this)).SendDocument(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task EspapMiddleware.ConsoleApp.LocalSVFReference.IService.SendDocumentAsync(EspapMiddleware.ConsoleApp.LocalSVFReference.SendDocumentRequest request) {
            return base.Channel.SendDocumentAsync(request);
        }
        
        public System.Threading.Tasks.Task SendDocumentAsync(EspapMiddleware.ConsoleApp.LocalSVFReference.SendDocumentContract SendDocumentMCIn) {
            EspapMiddleware.ConsoleApp.LocalSVFReference.SendDocumentRequest inValue = new EspapMiddleware.ConsoleApp.LocalSVFReference.SendDocumentRequest();
            inValue.SendDocumentMCIn = SendDocumentMCIn;
            return ((EspapMiddleware.ConsoleApp.LocalSVFReference.IService)(this)).SendDocumentAsync(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        void EspapMiddleware.ConsoleApp.LocalSVFReference.IService.SetDocumentResult(EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultRequest request) {
            base.Channel.SetDocumentResult(request);
        }
        
        public void SetDocumentResult(EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultContract SetDocumentResultMCIn) {
            EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultRequest inValue = new EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultRequest();
            inValue.SetDocumentResultMCIn = SetDocumentResultMCIn;
            ((EspapMiddleware.ConsoleApp.LocalSVFReference.IService)(this)).SetDocumentResult(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task EspapMiddleware.ConsoleApp.LocalSVFReference.IService.SetDocumentResultAsync(EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultRequest request) {
            return base.Channel.SetDocumentResultAsync(request);
        }
        
        public System.Threading.Tasks.Task SetDocumentResultAsync(EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultContract SetDocumentResultMCIn) {
            EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultRequest inValue = new EspapMiddleware.ConsoleApp.LocalSVFReference.SetDocumentResultRequest();
            inValue.SetDocumentResultMCIn = SetDocumentResultMCIn;
            return ((EspapMiddleware.ConsoleApp.LocalSVFReference.IService)(this)).SetDocumentResultAsync(inValue);
        }
    }
}
