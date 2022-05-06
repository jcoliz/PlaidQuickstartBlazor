using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaidQuickstartBlazor.Shared;

public class LinkResult
{
    public class t_metadata
    {
        public class t_institution
        {
            public string name { get; set; } = string.Empty;
            public string institution_id { get; set; } = string.Empty;
        }

        public string? link_session_id { get; set; }
        public string? status { get; set; }
        public string? request_id { get; set; }
        public t_institution? institution { get; set; }
    }

    public class t_error
    {
        public string error_type { get; set; } = string.Empty;
        public string error_code { get; set; } = string.Empty;
        public string error_message { get; set; } = string.Empty;
        public string? display_message { get; set; }
    }

    public bool ok { get; set; }
    public string public_token { get; set; } = string.Empty;
    public t_error? error { get; set; }
    public t_metadata? metadata { get; set; }
};