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
                ImageUrl = video.Snippet.Thumbnails.Standard.Url
            };
        }

        public async Task<List<TrackDto>> GetTracksAsync(List<string> trackIds)
        {
            var request = _ytService.Videos.List("snippet,statistics,contentDetails,topicDetails");
            request.Id = string.Join(",", trackIds);
            var response = await request.ExecuteAsync();

            return response.Items.Select(video => new TrackDto
            {
                TrackId = video.Id,
                CategoryId = int.Parse(video.Snippet.CategoryId),
                Title = video.Snippet.Title,
                ChannelTitle = video.Snippet.ChannelTitle,
                ViewCount = (long)video.Statistics.ViewCount,
                LikeCount = (long)video.Statistics.LikeCount,
                Duration = XmlConvert.ToTimeSpan(video.ContentDetails.Duration),
                ImageUrl = video.Snippet.Thumbnails.Standard.Url
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
        
        public async Task<List<string>> GetPlaylistVideoIdsAsync(string playlistId)
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
