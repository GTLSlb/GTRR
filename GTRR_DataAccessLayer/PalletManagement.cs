using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;


namespace GTRR_DataAccessLayer
{
    [Table("PalletManagement", Schema = "dbo")]
    public class PalletManagement
    {
        [Key]
        [JsonPropertyName("PalletMgmtId")]
        public int PALLET_MGMT_ID { get; set; }

        [JsonPropertyName("PalletMgmtName")]
        public string? PALLET_MGMT_NAME { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public int STATUS_ID { get; set; }
    }
}
