﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4200
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: global::System.Data.Objects.DataClasses.EdmSchemaAttribute()]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("BookLibraryModel", "FK_Person_Book", "Person", global::System.Data.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BookLibrary.Domain.Person), "Book", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BookLibrary.Domain.Book))]

// Original file name:
// Generation date: 11.02.2010 19:55:57
namespace BookLibrary.Domain
{
    
    /// <summary>
    /// There are no comments for BookLibraryEntities in the schema.
    /// </summary>
    public partial class BookLibraryEntities : global::System.Data.Objects.ObjectContext
    {
        /// <summary>
        /// Initializes a new BookLibraryEntities object using the connection string found in the 'BookLibraryEntities' section of the application configuration file.
        /// </summary>
        public BookLibraryEntities() : 
                base("name=BookLibraryEntities", "BookLibraryEntities")
        {
            this.OnContextCreated();
        }
        /// <summary>
        /// Initialize a new BookLibraryEntities object.
        /// </summary>
        public BookLibraryEntities(string connectionString) : 
                base(connectionString, "BookLibraryEntities")
        {
            this.OnContextCreated();
        }
        /// <summary>
        /// Initialize a new BookLibraryEntities object.
        /// </summary>
        public BookLibraryEntities(global::System.Data.EntityClient.EntityConnection connection) : 
                base(connection, "BookLibraryEntities")
        {
            this.OnContextCreated();
        }
        partial void OnContextCreated();
        /// <summary>
        /// There are no comments for Books in the schema.
        /// </summary>
        public global::System.Data.Objects.ObjectQuery<Book> Books
        {
            get
            {
                if ((this._Books == null))
                {
                    this._Books = base.CreateQuery<Book>("[Books]");
                }
                return this._Books;
            }
        }
        private global::System.Data.Objects.ObjectQuery<Book> _Books;
        /// <summary>
        /// There are no comments for Persons in the schema.
        /// </summary>
        public global::System.Data.Objects.ObjectQuery<Person> Persons
        {
            get
            {
                if ((this._Persons == null))
                {
                    this._Persons = base.CreateQuery<Person>("[Persons]");
                }
                return this._Persons;
            }
        }
        private global::System.Data.Objects.ObjectQuery<Person> _Persons;
        /// <summary>
        /// There are no comments for Books in the schema.
        /// </summary>
        public void AddToBooks(Book book)
        {
            base.AddObject("Books", book);
        }
        /// <summary>
        /// There are no comments for Persons in the schema.
        /// </summary>
        public void AddToPersons(Person person)
        {
            base.AddObject("Persons", person);
        }
    }
    /// <summary>
    /// There are no comments for BookLibraryModel.Book in the schema.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="BookLibraryModel", Name="Book")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Book : global::System.Data.Objects.DataClasses.EntityObject
    {
        /// <summary>
        /// Create a new Book object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        /// <param name="title">Initial value of Title.</param>
        /// <param name="author">Initial value of Author.</param>
        /// <param name="pages">Initial value of Pages.</param>
        public static Book CreateBook(global::System.Guid id, string title, string author, int pages)
        {
            Book book = new Book();
            book.Id = id;
            book.Title = title;
            book.Author = author;
            book.Pages = pages;
            return book;
        }
        /// <summary>
        /// There are no comments for Property Id in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Guid Id
        {
            get
            {
                return this._Id;
            }
            set
            {
                this.OnIdChanging(value);
                this.ReportPropertyChanging("Id");
                this._Id = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("Id");
                this.OnIdChanged();
            }
        }
        private global::System.Guid _Id;
        partial void OnIdChanging(global::System.Guid value);
        partial void OnIdChanged();
        /// <summary>
        /// There are no comments for Property Title in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Title
        {
            get
            {
                return this._Title;
            }
            set
            {
                this.OnTitleChanging(value);
                this.ReportPropertyChanging("Title");
                this._Title = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, false);
                this.ReportPropertyChanged("Title");
                this.OnTitleChanged();
            }
        }
        private string _Title;
        partial void OnTitleChanging(string value);
        partial void OnTitleChanged();
        /// <summary>
        /// There are no comments for Property Author in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Author
        {
            get
            {
                return this._Author;
            }
            set
            {
                this.OnAuthorChanging(value);
                this.ReportPropertyChanging("Author");
                this._Author = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, false);
                this.ReportPropertyChanged("Author");
                this.OnAuthorChanged();
            }
        }
        private string _Author;
        partial void OnAuthorChanging(string value);
        partial void OnAuthorChanged();
        /// <summary>
        /// There are no comments for Property Publisher in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Publisher
        {
            get
            {
                return this._Publisher;
            }
            set
            {
                this.OnPublisherChanging(value);
                this.ReportPropertyChanging("Publisher");
                this._Publisher = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Publisher");
                this.OnPublisherChanged();
            }
        }
        private string _Publisher;
        partial void OnPublisherChanging(string value);
        partial void OnPublisherChanged();
        /// <summary>
        /// There are no comments for Property PublishDate in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> PublishDate
        {
            get
            {
                return this._PublishDate;
            }
            set
            {
                this.OnPublishDateChanging(value);
                this.ReportPropertyChanging("PublishDate");
                this._PublishDate = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("PublishDate");
                this.OnPublishDateChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _PublishDate;
        partial void OnPublishDateChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnPublishDateChanged();
        /// <summary>
        /// There are no comments for Property Isbn in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Isbn
        {
            get
            {
                return this._Isbn;
            }
            set
            {
                this.OnIsbnChanging(value);
                this.ReportPropertyChanging("Isbn");
                this._Isbn = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Isbn");
                this.OnIsbnChanged();
            }
        }
        private string _Isbn;
        partial void OnIsbnChanging(string value);
        partial void OnIsbnChanged();
        /// <summary>
        /// There are no comments for Property Pages in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public int Pages
        {
            get
            {
                return this._Pages;
            }
            set
            {
                this.OnPagesChanging(value);
                this.ReportPropertyChanging("Pages");
                this._Pages = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("Pages");
                this.OnPagesChanged();
            }
        }
        private int _Pages;
        partial void OnPagesChanging(int value);
        partial void OnPagesChanged();
        /// <summary>
        /// There are no comments for Property LanguageInternal in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        private int LanguageInternal
        {
            get
            {
                return this._LanguageInternal;
            }
            set
            {
                this.OnLanguageInternalChanging(value);
                this.ReportPropertyChanging("LanguageInternal");
                this._LanguageInternal = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("LanguageInternal");
                this.OnLanguageInternalChanged();
            }
        }
        private int _LanguageInternal;
        partial void OnLanguageInternalChanging(int value);
        partial void OnLanguageInternalChanged();
        /// <summary>
        /// There are no comments for LendTo in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("BookLibraryModel", "FK_Person_Book", "Person")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public Person LendTo
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedReference<Person>("BookLibraryModel.FK_Person_Book", "Person").Value;
            }
            set
            {
                ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedReference<Person>("BookLibraryModel.FK_Person_Book", "Person").Value = value;
            }
        }
        /// <summary>
        /// There are no comments for LendTo in the schema.
        /// </summary>
        [global::System.ComponentModel.BrowsableAttribute(false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityReference<Person> LendToReference
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedReference<Person>("BookLibraryModel.FK_Person_Book", "Person");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedReference<Person>("BookLibraryModel.FK_Person_Book", "Person", value);
                }
            }
        }
    }
    /// <summary>
    /// There are no comments for BookLibraryModel.Person in the schema.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="BookLibraryModel", Name="Person")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Person : global::System.Data.Objects.DataClasses.EntityObject
    {
        /// <summary>
        /// Create a new Person object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        /// <param name="firstname">Initial value of Firstname.</param>
        /// <param name="lastname">Initial value of Lastname.</param>
        public static Person CreatePerson(global::System.Guid id, string firstname, string lastname)
        {
            Person person = new Person();
            person.Id = id;
            person.Firstname = firstname;
            person.Lastname = lastname;
            return person;
        }
        /// <summary>
        /// There are no comments for Property Id in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Guid Id
        {
            get
            {
                return this._Id;
            }
            set
            {
                this.OnIdChanging(value);
                this.ReportPropertyChanging("Id");
                this._Id = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("Id");
                this.OnIdChanged();
            }
        }
        private global::System.Guid _Id;
        partial void OnIdChanging(global::System.Guid value);
        partial void OnIdChanged();
        /// <summary>
        /// There are no comments for Property Firstname in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Firstname
        {
            get
            {
                return this._Firstname;
            }
            set
            {
                this.OnFirstnameChanging(value);
                this.ReportPropertyChanging("Firstname");
                this._Firstname = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, false);
                this.ReportPropertyChanged("Firstname");
                this.OnFirstnameChanged();
            }
        }
        private string _Firstname;
        partial void OnFirstnameChanging(string value);
        partial void OnFirstnameChanged();
        /// <summary>
        /// There are no comments for Property Lastname in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Lastname
        {
            get
            {
                return this._Lastname;
            }
            set
            {
                this.OnLastnameChanging(value);
                this.ReportPropertyChanging("Lastname");
                this._Lastname = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, false);
                this.ReportPropertyChanged("Lastname");
                this.OnLastnameChanged();
            }
        }
        private string _Lastname;
        partial void OnLastnameChanging(string value);
        partial void OnLastnameChanged();
        /// <summary>
        /// There are no comments for Property Email in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Email
        {
            get
            {
                return this._Email;
            }
            set
            {
                this.OnEmailChanging(value);
                this.ReportPropertyChanging("Email");
                this._Email = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Email");
                this.OnEmailChanged();
            }
        }
        private string _Email;
        partial void OnEmailChanging(string value);
        partial void OnEmailChanged();
        /// <summary>
        /// There are no comments for Borrowed in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("BookLibraryModel", "FK_Person_Book", "Book")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<Book> Borrowed
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<Book>("BookLibraryModel.FK_Person_Book", "Book");
            }
            private set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<Book>("BookLibraryModel.FK_Person_Book", "Book", value);
                }
            }
        }
    }
}