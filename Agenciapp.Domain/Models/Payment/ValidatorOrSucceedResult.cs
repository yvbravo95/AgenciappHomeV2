namespace AgenciappHome.Models.Payment
{
    public  class ValidatorOrSucceedResult<T>
    {
        public bool IsValidOrSucced { get; set; }
        public T Obj { get; set; }
        public string ErrorMessage { get; set; }
        
    }
}