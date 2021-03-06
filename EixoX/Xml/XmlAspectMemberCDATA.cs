﻿using EixoX.Adapters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EixoX.Xml
{
    public class XmlAspectMemberCDATA
        : XmlAspectMember
    {
        private SimpleAdapter _Adapter;

        public XmlAspectMemberCDATA(ClassAcessor acessor, string localName, bool mandatory, SimpleAdapter adapter)
            : base(acessor, localName, mandatory)
        {
            this._Adapter = adapter;
        }

        protected override void WriteXml(object entity, XmlElement parent, IFormatProvider formatProvider, string localName, bool mandatory)
        {
            object value = GetValue(entity);
            if (_Adapter.IsEmpty(value))
            {
                if (mandatory)
                {
                    XmlElement member = parent.OwnerDocument.CreateElement(localName);
                    parent.AppendChild(member);

                    string content = _Adapter.FormatObject(value, formatProvider);
                    if (!string.IsNullOrEmpty(content))
                        member.AppendChild(member.OwnerDocument.CreateCDataSection(content));
                }
            }
            else
            {
                XmlElement member = parent.OwnerDocument.CreateElement(localName);
                parent.AppendChild(member);

                string content = _Adapter.FormatObject(value, formatProvider);

                member.AppendChild(parent.OwnerDocument.CreateCDataSection(content));
            }

        }

        protected override void ReadXml(object entity, XmlElement parent, IFormatProvider formatProvider, string localName, bool mandatory)
        {
            XmlElement element = parent[localName];
            if (element != null)
            {
                string content = element.InnerText;
                object value = _Adapter.ParseObject(content, formatProvider );
                SetValue(entity, value);
            }
        }
    }
}
