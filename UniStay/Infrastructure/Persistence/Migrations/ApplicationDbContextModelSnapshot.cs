﻿// <auto-generated />
using System;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AmenityListing", b =>
                {
                    b.Property<Guid>("AmenitiesId")
                        .HasColumnType("uuid")
                        .HasColumnName("amenities_id");

                    b.Property<Guid>("ListingsId")
                        .HasColumnType("uuid")
                        .HasColumnName("listings_id");

                    b.HasKey("AmenitiesId", "ListingsId")
                        .HasName("pk_listing_amenities");

                    b.HasIndex("ListingsId")
                        .HasDatabaseName("ix_listing_amenities_listings_id");

                    b.ToTable("ListingAmenities", (string)null);
                });

            modelBuilder.Entity("Domain.Amenities.Amenity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("pk_amenities");

                    b.ToTable("amenities", (string)null);
                });

            modelBuilder.Entity("Domain.Favorites.Favorite", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("ListingId")
                        .HasColumnType("uuid")
                        .HasColumnName("listing_id");

                    b.HasKey("Id")
                        .HasName("pk_favorites");

                    b.HasIndex("ListingId")
                        .HasDatabaseName("ix_favorites_listing_id");

                    b.ToTable("Favorites", (string)null);
                });

            modelBuilder.Entity("Domain.ListingImages.ListingImage", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("character varying(2048)")
                        .HasColumnName("image_url");

                    b.Property<Guid>("ListingId")
                        .HasColumnType("uuid")
                        .HasColumnName("listing_id");

                    b.HasKey("Id")
                        .HasName("pk_listing_images");

                    b.HasIndex("ListingId")
                        .HasDatabaseName("ix_listing_images_listing_id");

                    b.ToTable("ListingImages", (string)null);
                });

            modelBuilder.Entity("Domain.Listings.Listing", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("address");

                    b.PrimitiveCollection<int[]>("CommunalServices")
                        .IsRequired()
                        .HasColumnType("integer[]")
                        .HasColumnName("communal_services");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)")
                        .HasColumnName("description");

                    b.Property<int>("Neighbours")
                        .HasColumnType("integer")
                        .HasColumnName("neighbours");

                    b.Property<int>("Owners")
                        .HasColumnType("integer")
                        .HasColumnName("owners");

                    b.Property<float>("Price")
                        .HasColumnType("real")
                        .HasColumnName("price");

                    b.Property<DateTime>("PublicationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("publication_date");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("title");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_listings");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_listings_user_id");

                    b.ToTable("Listings", (string)null);
                });

            modelBuilder.Entity("Domain.Messages.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("ReceiverId")
                        .HasColumnType("uuid")
                        .HasColumnName("receiver_id");

                    b.Property<DateTime>("SendDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("send_date");

                    b.Property<Guid>("SenderId")
                        .HasColumnType("uuid")
                        .HasColumnName("sender_id");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)")
                        .HasColumnName("text");

                    b.HasKey("Id")
                        .HasName("pk_messages");

                    b.HasIndex("ReceiverId")
                        .HasDatabaseName("ix_messages_receiver_id");

                    b.HasIndex("SenderId")
                        .HasDatabaseName("ix_messages_sender_id");

                    b.ToTable("messages", (string)null);
                });

            modelBuilder.Entity("Domain.Reviews.Review", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)")
                        .HasColumnName("comment");

                    b.Property<Guid>("ListingId")
                        .HasColumnType("uuid")
                        .HasColumnName("listing_id");

                    b.Property<DateTime>("PublicationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("publication_date");

                    b.Property<int>("Rating")
                        .HasColumnType("integer")
                        .HasColumnName("rating");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_reviews");

                    b.HasIndex("ListingId")
                        .HasDatabaseName("ix_reviews_listing_id");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_reviews_user_id");

                    b.ToTable("reviews", (string)null);
                });

            modelBuilder.Entity("Domain.Users.User", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("email");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("first_name");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("last_name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("phone_number");

                    b.Property<string>("ProfileImage")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("profile_image");

                    b.Property<DateTime>("RegistrationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("registration_date");

                    b.Property<int>("Role")
                        .HasColumnType("integer")
                        .HasColumnName("role");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("FavoriteUser", b =>
                {
                    b.Property<Guid>("FavoritesId")
                        .HasColumnType("uuid")
                        .HasColumnName("favorites_id");

                    b.Property<Guid>("UsersId")
                        .HasColumnType("uuid")
                        .HasColumnName("users_id");

                    b.HasKey("FavoritesId", "UsersId")
                        .HasName("pk_user_favorites");

                    b.HasIndex("UsersId")
                        .HasDatabaseName("ix_user_favorites_users_id");

                    b.ToTable("UserFavorites", (string)null);
                });

            modelBuilder.Entity("AmenityListing", b =>
                {
                    b.HasOne("Domain.Amenities.Amenity", null)
                        .WithMany()
                        .HasForeignKey("AmenitiesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_listing_amenities_amenities_amenities_id");

                    b.HasOne("Domain.Listings.Listing", null)
                        .WithMany()
                        .HasForeignKey("ListingsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_listing_amenities_listings_listings_id");
                });

            modelBuilder.Entity("Domain.Favorites.Favorite", b =>
                {
                    b.HasOne("Domain.Listings.Listing", "Listing")
                        .WithMany("Favorites")
                        .HasForeignKey("ListingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_favorites_listings_listing_id");

                    b.Navigation("Listing");
                });

            modelBuilder.Entity("Domain.ListingImages.ListingImage", b =>
                {
                    b.HasOne("Domain.Listings.Listing", "Listing")
                        .WithMany("ListingImages")
                        .HasForeignKey("ListingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_listing_images_listings_listing_id");

                    b.Navigation("Listing");
                });

            modelBuilder.Entity("Domain.Listings.Listing", b =>
                {
                    b.HasOne("Domain.Users.User", "User")
                        .WithMany("Listings")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_listings_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Messages.Message", b =>
                {
                    b.HasOne("Domain.Users.User", "Receiver")
                        .WithMany("ReceivedMessages")
                        .HasForeignKey("ReceiverId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("fk_messages_users_receiver_id");

                    b.HasOne("Domain.Users.User", "Sender")
                        .WithMany("SentMessages")
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("fk_messages_users_sender_id");

                    b.Navigation("Receiver");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("Domain.Reviews.Review", b =>
                {
                    b.HasOne("Domain.Listings.Listing", "Listing")
                        .WithMany("Reviews")
                        .HasForeignKey("ListingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_reviews_listings_listing_id");

                    b.HasOne("Domain.Users.User", "User")
                        .WithMany("Reviews")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_reviews_users_user_id");

                    b.Navigation("Listing");

                    b.Navigation("User");
                });

            modelBuilder.Entity("FavoriteUser", b =>
                {
                    b.HasOne("Domain.Favorites.Favorite", null)
                        .WithMany()
                        .HasForeignKey("FavoritesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_favorites_favorites_favorites_id");

                    b.HasOne("Domain.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_favorites_users_users_id");
                });

            modelBuilder.Entity("Domain.Listings.Listing", b =>
                {
                    b.Navigation("Favorites");

                    b.Navigation("ListingImages");

                    b.Navigation("Reviews");
                });

            modelBuilder.Entity("Domain.Users.User", b =>
                {
                    b.Navigation("Listings");

                    b.Navigation("ReceivedMessages");

                    b.Navigation("Reviews");

                    b.Navigation("SentMessages");
                });
#pragma warning restore 612, 618
        }
    }
}
