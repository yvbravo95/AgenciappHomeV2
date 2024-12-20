namespace AgenciappHome.Models.Payment
{
    public enum VoidReason
    {
        [Description("fraud")]
        Fraud,

        [Description("user_cancel")]
        UserCancel,

        [Description("icc_rejected")]
        IccRejected,

        [Description("icc_card_removed")]
        IccCardRemoved,

        [Description("icc_no_confirmation")]
        IccNoConfirmation,

        [Description("pos_timeout")]
        PosTimeout,

    }
}