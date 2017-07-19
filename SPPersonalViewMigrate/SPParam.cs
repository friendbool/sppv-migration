namespace Microsoft.SharePoint.StsAdmin
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    internal class SPParam
    {
        private bool m_bEnabled;
        private bool m_bIsFlag;
        private bool m_bIsRequired;
        private bool m_bUserTypedIn;
        private string m_strDefaultValue;
        private string m_strHelpMessage;
        private string m_strName;
        private string m_strShortName;
        private string m_strValue;
        private ISPValidator m_Validator;
        private ArrayList m_Validators;

        public SPParam(string strName, string strShortName)
        {
            this.m_strName = strName;
            this.m_strShortName = strShortName;
            this.m_bIsFlag = true;
            this.m_bEnabled = true;
        }

        public SPParam(string strName, string strShortName, bool bIsRequired, string strDefaultValue, ISPValidator validator) : this(strName, strShortName, bIsRequired, strDefaultValue, validator, "")
        {
        }

        public SPParam(string strName, string strShortName, bool bIsRequired, string strDefaultValue, ISPValidator validator, string strHelpMessage)
        {
            this.m_strName = strName;
            this.m_strShortName = strShortName;
            this.m_bIsRequired = bIsRequired;
            this.m_strDefaultValue = strDefaultValue;
            this.m_Validator = validator;
            this.m_strHelpMessage = strHelpMessage;
            this.m_bEnabled = true;
        }

        public SPParam(string strName, string strShortName, bool bIsRequired, string strDefaultValue, ArrayList validators, string strHelpMessage)
        {
            this.m_strName = strName;
            this.m_strShortName = strShortName;
            this.m_bIsRequired = bIsRequired;
            this.m_strDefaultValue = strDefaultValue;
            this.m_strHelpMessage = strHelpMessage;
            this.m_bEnabled = true;
            this.m_Validators = validators;
        }

        public void InitValueFrom(StringDictionary keyValues)
        {
            this.m_strValue = keyValues[this.Name];
            if (this.m_strValue == null)
            {
                this.m_strValue = keyValues[this.ShortName];
            }
            this.m_bUserTypedIn = this.m_strValue != null;
        }

        public bool Validate()
        {
            if (this.m_bIsFlag)
            {
                if ((this.m_strValue != null) && (this.m_strValue.Trim().Length != 0))
                {
                    return false;
                }
                return true;
            }
            if (this.m_Validator != null)
            {
                if (!this.m_Validator.Validate(this.Value))
                {
                    return false;
                }
            }
            else if (this.m_Validators != null)
            {
                foreach (ISPValidator validator in this.m_Validators)
                {
                    if (!validator.Validate(this.Value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public string DefaultValue
        {
            get
            {
                return this.m_strDefaultValue;
            }
        }

        public bool Enabled
        {
            get
            {
                return this.m_bEnabled;
            }
            set
            {
                this.m_bEnabled = value;
            }
        }

        public string HelpMessage
        {
            get
            {
                return this.m_strHelpMessage;
            }
        }

        public bool IsFlag
        {
            get
            {
                return this.m_bIsFlag;
            }
        }

        public bool IsRequired
        {
            get
            {
                return this.m_bIsRequired;
            }
            set
            {
                this.m_bIsRequired = value;
            }
        }

        public string Name
        {
            get
            {
                return this.m_strName;
            }
        }

        public string ShortName
        {
            get
            {
                return this.m_strShortName;
            }
        }

        public bool UserTypedIn
        {
            get
            {
                return this.m_bUserTypedIn;
            }
        }

        public string Value
        {
            get
            {
                if (!this.UserTypedIn)
                {
                    return this.m_strDefaultValue;
                }
                return this.m_strValue;
            }
        }
    }
}

