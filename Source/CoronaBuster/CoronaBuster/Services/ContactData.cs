using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using CoronaBuster.Models;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace CoronaBuster.Services {
    public class ContactData {
        public ObservableCollection<Contact> Contacts { get; private set; } = new ObservableCollection<Contact>();

        private PublicData _publicData = DependencyService.Get<PublicData>();

        private System.IO.Stream _stream;

        public ContactData() {
            _stream = DependencyService.Get<IFileIO>().OpenWrite(nameof(ContactData));
            ProtoBuf.Serializer.DeserializeItems<Contact>(_stream, ProtoBuf.PrefixStyle.Base128, 0).ForEach(c => Contacts.Add(c));
             
            _publicData.ContactFound += ContactFound;
        }

        private void ContactFound(Contact contact) {
            ProtoBuf.Serializer.SerializeWithLengthPrefix(_stream, contact, ProtoBuf.PrefixStyle.Base128, 0);
            _stream.Flush();

            Contacts.Add(contact);
        }
    }
}
