using Google.Apis.YouTube.v3;
using System.Xml;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.Track;
using YTMusicApi.Model.YouTube;

namespace YTMusicApi.Data.YouTube
{
    public class YouTubeRepository : IYouTubeRepository
    {
        private readonly YouTubeService _ytService;

        public YouTubeRepository(YouTubeService ytService)
        {
            _ytService = ytService;
        }

        public async Task<TrackDto> GetTrackAsync(string trackId)
        {
            var request = _ytService.Videos.List("snippet,statistics,contentDetails,topicDetails");
            request.Id = trackId;
            var response = await request.ExecuteAsync();

            var video = response.Items.FirstOrDefault();
            if (video == null) return null;

            return new TrackDto
            {
                TrackId = trackId,
                CategoryId = int.Parse(video.Snippet.CategoryId),
                Title = video.Snippet.Title,
                ChannelTitle = video.Snippet.ChannelTitle,
                ViewCount = (long)video.Statistics.ViewCount,
                LikeCount = (long)video.Statistics.LikeCount,
                Duration = XmlConvert.ToTimeSpan(video.ContentDetails.Duration),
                ImageUrl = video.Snippet.Thumbnails.Medium.Url
            };
        }

        public async Task<List<TrackDto>> GetTracksAsync(List<string> trackIds)
        {
            var request = _ytService.Videos.List("snippet,statistics,contentDetails,topicDetails");
            request.Id = string.Join(",", trackIds);
            var response = await request.ExecuteAsync();

            return response.Items.Select(track => new TrackDto
            {
                TrackId = track.Id,
                CategoryId = int.Parse(track.Snippet.CategoryId),
                Title = track.Snippet.Title,
                ChannelTitle = track.Snippet.ChannelTitle,
                ViewCount = (long)track.Statistics.ViewCount,
                LikeCount = (long)track.Statistics.LikeCount,
                Duration = XmlConvert.ToTimeSpan(track.ContentDetails.Duration),
                ImageUrl = track.Snippet.Thumbnails.Medium.Url
            }).ToList();
        }

        public async Task<PlaylistDto> GetPlaylistAsync(string playlistId)
        {
            var request = _ytService.Playlists.List("snippet,contentDetails");
            request.Id = playlistId;
            var response = await request.ExecuteAsync();
            var playlist = response.Items.FirstOrDefault();
            if (playlist == null) return null;
            return new PlaylistDto
            {
                PlaylistId = playlist.Id,
                Title = playlist.Snippet.Title,
                СhannelTitle = playlist.Snippet.ChannelTitle,
                ItemCount = (int)playlist.ContentDetails.ItemCount
            };
        }

        public async Task<List<PlaylistDto>> GetPlaylistsAsync(List<string> playlistIds)
        {
            var request = _ytService.Playlists.List("snippet,contentDetails");
            request.Id = string.Join(",", playlistIds);
            var response = await request.ExecuteAsync();

            return response.Items.Select(playlist => new PlaylistDto
            {
                PlaylistId = playlist.Id,
                Title = playlist.Snippet.Title,
                СhannelTitle = playlist.Snippet.ChannelTitle,
                ItemCount = (int)playlist.ContentDetails.ItemCount
            }).ToList();
        }

        public async Task<List<string>> GetTrackIdsFromPlaylistAsync(string playlistId)
        {
            var videoIds = new List<string>();
            string nextPageToken = null;

            do
            {
                var plRequest = _ytService.PlaylistItems.List("snippet");
                plRequest.PlaylistId = playlistId;
                plRequest.MaxResults = 50;
                plRequest.PageToken = nextPageToken;

                var plResponse = await plRequest.ExecuteAsync();
                if (plResponse.Items == null) break;

                videoIds.AddRange(plResponse.Items
                    .Select(item => item.Snippet?.ResourceId?.VideoId)
                    .Where(id => !string.IsNullOrEmpty(id)));

                nextPageToken = plResponse.NextPageToken;
            }
            while (!string.IsNullOrEmpty(nextPageToken));

            return videoIds;
        }
    }
}
