using System;
using System.Collections.Generic;

namespace Smdn.Protocols.Imap4 {
  public class ImapServerNamespace {
    public ImapNamespace[] PersonalNamespaces {
      get { return personalNamespaces; }
    }

    public ImapNamespace[] OtherUsersNamespaces {
      get { return otherUsersNamespaces; }
    }

    public ImapNamespace[] SharedNamespaces {
      get { return sharedNamespaces; }
    }

    public ImapServerNamespace()
      : this(new ImapNamespace[] {}, new ImapNamespace[] {}, new ImapNamespace[] {})
    {
    }

    public ImapServerNamespace(ImapNamespace[] personalNamespaces, ImapNamespace[] otherUsersNamespaces, ImapNamespace[] sharedNamespaces)
    {
      if (personalNamespaces == null)
        throw new ArgumentNullException("personalNamespaces");
      if (otherUsersNamespaces == null)
        throw new ArgumentNullException("otherUsersNamespaces");
      if (sharedNamespaces == null)
        throw new ArgumentNullException("sharedNamespaces");

      this.personalNamespaces = personalNamespaces;
      this.otherUsersNamespaces = otherUsersNamespaces;
      this.sharedNamespaces = sharedNamespaces;
    }

    public override string ToString()
    {
      return string.Format("{{PersonalNamespaces={0}, OtherUsersNamespaces={1}, SharedNamespaces={2}}}",
                           string.Join(", ", Array.ConvertAll(personalNamespaces,   delegate(ImapNamespace ns) { return ns.ToString(); })),
                           string.Join(", ", Array.ConvertAll(otherUsersNamespaces, delegate(ImapNamespace ns) { return ns.ToString(); })),
                           string.Join(", ", Array.ConvertAll(sharedNamespaces,     delegate(ImapNamespace ns) { return ns.ToString(); })));
    }

    private /*readonly*/ ImapNamespace[] personalNamespaces;
    private /*readonly*/ ImapNamespace[] otherUsersNamespaces;
    private /*readonly*/ ImapNamespace[] sharedNamespaces;
  }
}
