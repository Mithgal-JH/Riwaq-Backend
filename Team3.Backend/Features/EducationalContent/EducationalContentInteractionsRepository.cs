using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.EducationalContent.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.EducationalContent;

public class EducationalContentInteractionsRepository
    : IEducationalContentInteractionsRepository
{
    private readonly AppDbContext _context;

    public EducationalContentInteractionsRepository(AppDbContext context)
    {
        _context = context;
    }

    // Get an existing like for a user and educational content.
    public async Task<Like?> GetLikeAsync(
        Guid userId,
        Guid educationalContentId)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(like =>
                like.UserId == userId
                && like.EducationalContentId == educationalContentId);
    }

    // Add a new like.
    public void AddLike(Like like)
    {
        _context.Likes.Add(like);
    }

    // Remove an existing like.
    public void RemoveLike(Like like)
    {
        _context.Likes.Remove(like);
    }

    // Get an existing save for a user and educational content.
    public async Task<Save?> GetSaveAsync(
        Guid userId,
        Guid educationalContentId)
    {
        return await _context.Saves
            .FirstOrDefaultAsync(save =>
                save.UserId == userId
                && save.EducationalContentId == educationalContentId);
    }

    // Add a new save.
    public void AddSave(Save save)
    {
        _context.Saves.Add(save);
    }

    // Remove an existing save.
    public void RemoveSave(Save save)
    {
        _context.Saves.Remove(save);
    }

    // Get an existing repost for a user and educational content.
    public async Task<Repost?> GetRepostAsync(
        Guid userId,
        Guid educationalContentId)
    {
        return await _context.Reposts
            .FirstOrDefaultAsync(repost =>
                repost.UserId == userId
                && repost.EducationalContentId == educationalContentId);
    }

    // Add a new repost.
    public void AddRepost(Repost repost)
    {
        _context.Reposts.Add(repost);
    }

    // Remove an existing repost.
    public void RemoveRepost(Repost repost)
    {
        _context.Reposts.Remove(repost);
    }
    // Add a new share event.
    public void AddShare(Share share)
    {
        _context.Shares.Add(share);
    }
    // Save changes to the database.
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}