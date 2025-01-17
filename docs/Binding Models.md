## Binding Models

- [Introduction](#introduction)
- [Creating a BindingModel](#creating-a-binding-model)
- [Retrieving the CRM data](#retrieving-the-crm-data)
- [Updating the CRM data](#updating-the-crm-data)
- [Json serialization](#json-serialization)
- [Types to use for CRM data](#types-to-use-for-crm-data)
- [Connecting to other BindingModels](#connecting-to-other-bindingmodels)
- [Structuring the data](#structuring-the-data)


##  Introduction
There are three classes related to a a table from a CRM at various levels of abstraction.
  - Entity, stores the data corresponding to a table record from the CRM.
  - EntityDefinition, lists the various components of a table. Each table can have one per project.
  - BindingModel, strongly typed representation of a table record. There can be as many binding models as there are needs for one table in one project.



## Creating a Binding Model
First, you need a Definition corresponding to your table. You need to retrieve it from the CRM. You can see how by following [this](https://github.com/cgoconseils/XrmFramework#readme) tutorial

Then create a new class in your project, it needs to inherit either IBindingModel or BindingModelBase the difference between the two is explained in [this](#updating-the-crm-data) part.
You can add a property for each column you want to use in your project.
To do so, use the CrmMapping attribute and the corresponding TableDefinition. The type of the property has to make sense for the column you want to retrieve. For example, a property corresponding to the name column of a table will have to be of type string. The various types to use are detailed in [this](#types-to-use-for-crm-data) section.

```CS

    [CrmEntity(AccountDefinition.EntityName)] //Specifies the table to which the model maps 
    public class AccountModel : IBindingModel // Can be replaced with BindingModelBase
    {
        public Guid Id { get; set; } // Corresponds to the unique identifier of the record
        
        [CrmMapping(AccountDefinition.Columns.Name)] // Specifies the column to which the property maps
        public string Name { get; set; }
        
    }
```




## Retrieving the CRM data
In order to retrieve table records as BindingModel, the framework uses custom AdminOrganizationService functions : 
```cs
query = BindingModelHelper.GetRetrieveAllQuery<AccountModel>();
AdminOrganizationService.RetrieveAll<BindingModel>(query); // Returns all records corresponding to the table present on the CRM as BindingModels.

AdminOrganizationService.GetById<AccountModel>(ID); // Returns the table record corresponding to the ID as a BindingModel

```

If you want to retrieve the data corresponding to a lookup property in the form of a model, you should use the FollowLink option for the CrmMapping Attribute.
```cs
[CrmMapping(ContactDefinition.Columns.FirstLeadId, FollowLink = true)]
public LeadModel FirstLead {get;set;}
```





## Updating the CRM data

In order to update the data using a BindingModel you can use the Upsert function. However, to avoid any chance of overwriting CRM data, we recommend the following steps : 
Make it so that your BindingModel inherits BindingModelBase and then call the OnPropertyChanged function inside of the set function of any property you wish to be able to update on the CRM. Then create the difference between the CRM record and the local record by using the GetDiffGeneric function. Then use the Upsert function on the result.

```cs
var existingAccount = service.getById<AccountModel>(accountID);
var newAccountModel = new AccountModel {Name = "Titi"};
var diffAccount = newAccountModel.GetDiffGeneric(existingAccount);
if(diffAccount.InitializedProperties.Any())
{
    service.Upsert(diffAccount);
}
```

Another way to control the way a CRM record is updated is to create a custom Upsert behavior. For example, if your model contains a custom property that connects to a list of other BindingModels, you can add the upsert of these model in your custom behavior. An UpsertBehavior is a class that implements the IBehavior interface.

```cs
public class MyUpsertBehavior : IBehavior<MyModel>
{
  public void ApplyBehavior(IOrganizationService service, MyModel model)
  {
    // Put your custom logic here
  }

}
```


```cs
[CrmEntity(MyDefinition.EntityName)]
[UpsertBehavior(typeof(MyUpsertBehavior))]
public class MyModel : BindingModelBase

```

## JSon Serialization
Any BindingModel instance can be serialized by using the JsonProperty and JsonObject attributes :
```cs

[JsonObject(MemberSerialization.OptIn)]
[CrmEntity(BaseDefinition.EntityName)]
public class BaseModel : IBindingModel

//////////////////////////////////////////////////////////////////////////
[JsonProperty("name")]
[CrmMapping(BaseDefinition.Columns.Name)]
public string Name {get;set;}
```

If a property is of a complex type such as another BindingModel, you can use a custom type converter.
```cs
[JsonConverter(typeof(MyCustomConverter))]
```



## Types to use for CRM data
  
  | CRM column type      | C# equivalents |
  | ----------- | ----------- |
  | Boolean   | System.Boolean       |
  |    | System.Int32       |
  |    | System.String       |
  | Integer   | System.Int32       |
  | DateTime   | System.DateTime       |
  | Decimal   | System.Decimal       |
  | Double   | System.Double       |
  | Lookup   | To be explained further       |
  | Memo   | System.String       |
  | PickList   | System.Int32       |
  | PickList   | Corresponding OptionSetEnum       |
  | State   | Corresponding OptionSetEnum       |
  | State   | System.Int32       |
  | Status   | Corresponding OptionSetEnum       |
  | Status   | System.Int32       |
  | String   | System.String       |
  | UniqueIdentifier   | System.Guid       |
  | BigInt   | System.Int64       |
  | EntityName   | System.String       |


Lookup et oneToManyRelationShip 
  
  
## Connecting to other BindingModels
An Entity attribute of type Lookup can be used to retrieve :
- the data corresponding to the ID of the corresponding Entity record, 
```cs
[CrmMapping(AccountDefinition.Columns.PrimaryContactId)]
public Guid PrimaryContactIdentifier {get;set;}
```
- the EntityReference instance of the corresponding Entity record,
```cs
[CrmMapping(AccountDefinition.Columns.PrimaryContactId)]
public EntityReference PrimaryContactReference {get;set;}
```
- the data of a particular attribute of the corresponding Entity record by using the CrmLookup attribute
```cs
[CrmMapping(AccountDefinition.Columns.PrimaryContactId)]
[CrmLookup(ContactDefinition.EntityName,ContactDefinition.Columns.Name)]
public string PrimaryContactName {get;set;} // Corresponds to the name of the record corresponding to the primary contact of the account record
```
- the data of a collection of attributes through the BindingModel instance of the corresponding Entity record
```cs
[CrmMapping(AccountDefinition.Columns.PrimaryContactId)]
public ContactModel PrimaryContact {get;set;}

// [CrmEntity(ContactDefinition.EntityName)]
// public ContactModel : IBindingModel
// {
//   [CrmMapping(ContactDefinition.Columns.Name)]
//   public string Name {get;set;}
// }

```

You can also have a list of BindingModel instances by using a OneToMany relationship of the table.

```cs
[ChildRelationship(AccountDefinition.OneToManyRelationShip.contact_customer_accounts)]
public ICollection<ContactModel> SubContacts {get;} = new List<ContactModel>();

```

## Structuring the data
You can regroup several table columns together under one property by using the ExtendBindingModel attribute. You can do so by creating a second model for the same table, in this model, map the attributes you want to see grouped together. Then for your first model, add a property with the second BindingModel and use the ExtendBindingModel attribute.


<table>
<tr>
<th>Code</th>
<th>Corresponding data structure</th>
</tr>
<tr>
<td>
  
```cs
    [JsonObject(MemberSerialization.OptIn)]
    [CrmEntity(AccountDefinition.EntityName)]
    public class AccountModel : IBindingModel
    {
    [JsonProperty("id")]
    public Guid Id {get;set;}
    
    [CrmMapping(AccountEntity.Columns.PrimaryContactId)]
    public EntityReference PrimaryContactRef {get;set;}
    
    [JsonProperty("name")]
    [CrmMapping(AccountEntity.Columns.Name)]
    public string Name {get;set;}
    
    [JsonProperty("Address1Line")]
    [CrmMapping(AccountEntity.Columns.Address1_Line1)]
    public string AddressLine1 {get;set;}
    
    [JsonProperty("Address1City")]
    [CrmMapping(AccountEntity.Columns.Address1_City)]
    public string AddressLine1 {get;set;}
    
    }
```



  

  
  
</td>
<td>

```json
{
  "id": "0f8fad5b-d9cb-469f-a165-70867728950e",
  "name": "mary",
  "Address1City": "San York",
  "Address1Line": "23 Rupert Street"
}
```

</td>
</tr>
  
<tr>
<td>

  ```cs
    [JsonObject(MemberSerialization.OptIn)]
    [CrmEntity(AccountDefinition.EntityName)]
    public class AccountModel : IBindingModel
    {
    [JsonProperty("id")]
    public Guid Id {get;set;}
    
    [CrmMapping(AccountEntity.Columns.PrimaryContactId)]
    public EntityReference PrimaryContactRef {get;set;}
    
    [JsonProperty("name")]
    [CrmMapping(AccountEntity.Columns.Name)]
    public string Name {get;set;}
    
    [JsonProperty("Address")]
    [ExtendBindingModel]
    public AccountAddressModel Address {get;set;}
    }
    
    // Second BindingModel
    [JsonObject(MemberSerialization.OptIn)]
    [CrmEntity(AccountDefinition.EntityName)]
    public class AccountAddressModel : IBindingModel
    
    {
    [JsonProperty("id")]
    public Guid Id {get;set;}
    
   ///// Properties to be grouped
    [JsonProperty("Line")]
    [CrmMapping(AccountEntity.Columns.Address1_Line1)]
    public string AddressLine1 {get;set;}
    
    [JsonProperty("City")]
    [CrmMapping(AccountEntity.Columns.Address1_City)]
    public string AddressLine1 {get;set;}
   ///////
    }
```

</td>
<td>

```json
{
  "id": "0f8fad5b-d9cb-469f-a165-70867728950e",
  "name": "mary",
  "Address" : 
  {
    "City": "San York",
    "Line": "23 Rupert Street"
  }
}
```

</td> 
</tr>
</table>


