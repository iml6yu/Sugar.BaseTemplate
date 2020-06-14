using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sugar.BaseTemplate.Models
{
    public abstract class BaseTenantModel : BaseModel
    {
        public Guid? TenantId { get; set; }
        protected BaseTenantModel()
        {

        }
        protected BaseTenantModel(Guid id)
        {
            this.Id = id;
        }
    }

    public abstract class BaseModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [DefaultValue(false)]
        public bool IsDeleted { get; set; }
        public Guid? DeleterId { get; set; }
        public DateTime? DeletedTime { get; set; }

        public Guid? ModifierId { get; set; }
        public DateTime? LaseModifyTime { get; set; }


        public Guid CreateorId { get; set; } = Guid.Empty;
        public DateTime CreateTime { get; set; } = DateTime.Now;


        [Timestamp]
        public byte[] Timestamp { get; set; }
        /// <summary>
        /// 扩展数据 Json格式
        /// </summary>
        public string ExtendJson { get; set; }
        protected BaseModel()
        {

        }
        protected BaseModel(Guid id)
        {
            this.Id = id;
        }
    }
}
