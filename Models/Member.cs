using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GongChaWebApplication.Models
{
    public class Member
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Name { get; set; }

        [DisplayName("Access Level")]
        public string AccessLevel { get; set; }

        [DisplayName("Last Login")]
        public DateTime? LastLogin { get; set; }

        [DisplayName("Date of Joining")]
        public DateTime? JoinDate { get; set; }

        [DisplayName("Date of Leaving")]
        public DateTime? LeaveDate { get; set; }

        public string AuthCode { get; set; }

        public virtual Store Store { get; set; }

        public virtual ICollection<Sales> SalesItems { get; set; }

        [DisplayName("State")]
        public virtual MemberState MemberState { get { return (MemberState)this.stateValue; } set { this.stateValue = (int)value; } }
        public int stateValue { get; set; }

    }
    public static class MemberAccessLevels
    {

        public const string DIRECTOR = "director";
        public const string MANAGER = "manager";
        public const string STAFF = "staff";
        public const string WAREHOUSE_MANAGER = "warehouse manager";

    }
    public enum MemberState
    {
        ACTIVE,
        DELETED
    }
}