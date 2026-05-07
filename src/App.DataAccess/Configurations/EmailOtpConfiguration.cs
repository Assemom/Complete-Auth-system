using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.DataAccess.Configurations;

public class EmailOtpConfiguration : IEntityTypeConfiguration<EmailOtp>
{
    private const int CodeMaxLength = 6;
    private const int UserIdMaxLength = 450;

    public void Configure(EntityTypeBuilder<EmailOtp> builder)
    {
        builder.HasKey(otp => otp.Id);

        builder.Property(otp => otp.Code)
            .IsRequired()
            .HasMaxLength(CodeMaxLength);

        builder.Property(otp => otp.ExpiresAt)
            .IsRequired();

        builder.Property(otp => otp.IsUsed)
            .IsRequired();

        builder.Property(otp => otp.UserId)
            .IsRequired()
            .HasMaxLength(UserIdMaxLength);

        builder.HasOne(otp => otp.User)
            .WithMany()
            .HasForeignKey(otp => otp.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
