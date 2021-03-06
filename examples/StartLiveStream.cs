using System;
using System.Collections.Generic;
using System.Threading;
using com.bitmovin.Api.Codec;
using com.bitmovin.Api.Encoding;
using com.bitmovin.Api.Enums;
using com.bitmovin.Api.Input;
using com.bitmovin.Api.Manifest;
using com.bitmovin.Api.Output;
using Fmp4 = com.bitmovin.Api.Encoding.Fmp4;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace com.bitmovin.Api.Examples
{

    [TestClass]
    public class StartLiveStream
    {
        private const string API_KEY = "YOUR API KEY";
        private const string GCS_ACCESS_KEY = "GCS ACCESS KEY";
        private const string GCS_SECRET_KEY = "GCS SECRET KEY";
        private const string GCS_BUCKET_NAME = "GCS BUCKET NAME";
        private const string OUTPUT_PATH = "path/to/output/";

        [TestMethod]
        public void StartLiveEncoding()
        {
            var bitmovin = new BitmovinApi(API_KEY);
            double? segmentLength = 4.0;

            var output = bitmovin.Output.Gcs.Create(new GcsOutput
            {
                Name = "GCS Ouput",
                AccessKey = GCS_ACCESS_KEY,
                SecretKey = GCS_SECRET_KEY,
                BucketName = GCS_BUCKET_NAME
            });

            var encoding = bitmovin.Encoding.Encoding.Create(new Encoding.Encoding
            {
                Name = "Live Stream C#",
                CloudRegion = EncodingCloudRegion.GOOGLE_EUROPE_WEST_1,
                EncoderVersion = "STABLE"
            });


            var rtmpInput = bitmovin.Input.Rtmp.RetrieveList(0, 100)[0];

            var videoConfig1080p = bitmovin.Codec.H264.Create(new H264VideoConfiguration
            {
                Name = "H264_Profile_1080p",
                Profile = H264Profile.HIGH,
                Width = 1920,
                Height = 1080,
                Bitrate = 4800000,
                Rate = 30.0f
            });
            var videoStream1080p = bitmovin.Encoding.Encoding.Stream.Create(encoding.Id,
                CreateStream(rtmpInput, "live", 0, videoConfig1080p, SelectionMode.AUTO));

            var videoConfig720p = bitmovin.Codec.H264.Create(new H264VideoConfiguration
            {
                Name = "H264_Profile_720p",
                Profile = H264Profile.HIGH,
                Width = 1280,
                Height = 720,
                Bitrate = 2400000,
                Rate = 30.0f
            });
            var videoStream720p = bitmovin.Encoding.Encoding.Stream.Create(encoding.Id,
                CreateStream(rtmpInput, "live", 0, videoConfig720p, SelectionMode.AUTO));

            var videoConfig480p = bitmovin.Codec.H264.Create(new H264VideoConfiguration
            {
                Name = "H264_Profile_480p",
                Profile = H264Profile.HIGH,
                Width = 858,
                Height = 480,
                Bitrate = 1200000,
                Rate = 30.0f
            });
            var videoStream480p = bitmovin.Encoding.Encoding.Stream.Create(encoding.Id,
                CreateStream(rtmpInput, "live", 0, videoConfig480p, SelectionMode.AUTO));

            var videoConfig360p = bitmovin.Codec.H264.Create(new H264VideoConfiguration
            {
                Name = "H264_Profile_360p",
                Profile = H264Profile.HIGH,
                Width = 640,
                Height = 360,
                Bitrate = 800000,
                Rate = 30.0f
            });
            var videoStream360p = bitmovin.Encoding.Encoding.Stream.Create(encoding.Id,
                CreateStream(rtmpInput, "live", 0, videoConfig360p, SelectionMode.AUTO));

            var videoConfig240p = bitmovin.Codec.H264.Create(new H264VideoConfiguration
            {
                Name = "H264_Profile_240p",
                Profile = H264Profile.HIGH,
                Width = 426,
                Height = 240,
                Bitrate = 400000,
                Rate = 30.0f
            });
            var videoStream240p = bitmovin.Encoding.Encoding.Stream.Create(encoding.Id,
                CreateStream(rtmpInput, "live", 0, videoConfig240p, SelectionMode.AUTO));

            var audioConfig = bitmovin.Codec.Aac.Create(new AACAudioConfiguration
            {
                Name = "AAC_Profile_128k",
                Bitrate = 128000,
                Rate = 48000
            });
            var audioStream = bitmovin.Encoding.Encoding.Stream.Create(encoding.Id,
                CreateStream(rtmpInput, "/", 1, audioConfig, SelectionMode.AUTO));

            var videoFMP4Muxing240p = bitmovin.Encoding.Encoding.Fmp4.Create(encoding.Id,
                CreateFMP4Muxing(videoStream240p, output, OUTPUT_PATH + "video/240p", segmentLength));
            var videoFMP4Muxing360p = bitmovin.Encoding.Encoding.Fmp4.Create(encoding.Id,
                CreateFMP4Muxing(videoStream360p, output, OUTPUT_PATH + "video/360p", segmentLength));
            var videoFMP4Muxing480p = bitmovin.Encoding.Encoding.Fmp4.Create(encoding.Id,
                CreateFMP4Muxing(videoStream480p, output, OUTPUT_PATH + "video/480p", segmentLength));
            var videoFMP4Muxing720p = bitmovin.Encoding.Encoding.Fmp4.Create(encoding.Id,
                CreateFMP4Muxing(videoStream720p, output, OUTPUT_PATH + "video/720p", segmentLength));
            var videoFMP4Muxing1080p = bitmovin.Encoding.Encoding.Fmp4.Create(encoding.Id,
                CreateFMP4Muxing(videoStream1080p, output, OUTPUT_PATH + "video/1080p", segmentLength));
            var audioFMP4Muxing = bitmovin.Encoding.Encoding.Fmp4.Create(encoding.Id,
                CreateFMP4Muxing(audioStream, output, OUTPUT_PATH + "audio/128kbps", segmentLength));


            var manifestOutput = new Encoding.Output
            {
                OutputPath = OUTPUT_PATH,
                OutputId = output.Id,
                Acl = new List<Acl> {new Acl {Permission = Permission.PUBLIC_READ}}
            };
            var manifest = bitmovin.Manifest.Dash.Create(new Dash
            {
                Name = "MPEG-DASH Manifest",
                ManifestName = "stream.mpd",
                Outputs = new List<Encoding.Output> {manifestOutput}
            });
            var period = bitmovin.Manifest.Dash.Period.Create(manifest.Id, new Period());
            var videoAdaptationSet =
                bitmovin.Manifest.Dash.VideoAdaptationSet.Create(manifest.Id, period.Id, new VideoAdaptationSet());
            var audioAdaptationSet = bitmovin.Manifest.Dash.AudioAdaptationSet.Create(manifest.Id, period.Id,
                new AudioAdaptationSet {Lang = "en"});

            bitmovin.Manifest.Dash.Fmp4.Create(manifest.Id, period.Id, audioAdaptationSet.Id,
                new Manifest.Fmp4
                {
                    Type = SegmentScheme.TEMPLATE,
                    EncodingId = encoding.Id,
                    MuxingId = audioFMP4Muxing.Id,
                    SegmentPath = "audio/128kbps"
                });

            bitmovin.Manifest.Dash.Fmp4.Create(manifest.Id, period.Id, videoAdaptationSet.Id,
                new Manifest.Fmp4
                {
                    Type = SegmentScheme.TEMPLATE,
                    EncodingId = encoding.Id,
                    MuxingId = videoFMP4Muxing240p.Id,
                    SegmentPath = "video/240p"
                });
            bitmovin.Manifest.Dash.Fmp4.Create(manifest.Id, period.Id, videoAdaptationSet.Id,
                new Manifest.Fmp4
                {
                    Type = SegmentScheme.TEMPLATE,
                    EncodingId = encoding.Id,
                    MuxingId = videoFMP4Muxing360p.Id,
                    SegmentPath = "video/360p"
                });
            bitmovin.Manifest.Dash.Fmp4.Create(manifest.Id, period.Id, videoAdaptationSet.Id,
                new Manifest.Fmp4
                {
                    Type = SegmentScheme.TEMPLATE,
                    EncodingId = encoding.Id,
                    MuxingId = videoFMP4Muxing480p.Id,
                    SegmentPath = "video/480p"
                });
            bitmovin.Manifest.Dash.Fmp4.Create(manifest.Id, period.Id, videoAdaptationSet.Id,
                new Manifest.Fmp4
                {
                    Type = SegmentScheme.TEMPLATE,
                    EncodingId = encoding.Id,
                    MuxingId = videoFMP4Muxing720p.Id,
                    SegmentPath = "video/720p"
                });
            bitmovin.Manifest.Dash.Fmp4.Create(manifest.Id, period.Id, videoAdaptationSet.Id,
                new Manifest.Fmp4
                {
                    Type = SegmentScheme.TEMPLATE,
                    EncodingId = encoding.Id,
                    MuxingId = videoFMP4Muxing1080p.Id,
                    SegmentPath = "video/1080p"
                });

            bitmovin.Encoding.Encoding.StartLive(encoding.Id, new StartLiveEncodingRequest
            {
                StreamKey = "YourStreamKey",
                DashManifests = new List<LiveDashManifest>
                {
                    new LiveDashManifest
                    {
                        ManifestId = manifest.Id,
                        Timeshift = 300,
                        LiveEdgeOffset = 90
                    }
                }
            });

            LiveEncoding liveEncoding = null;

            while (liveEncoding == null)
            {
                try
                {
                    liveEncoding = bitmovin.Encoding.Encoding.RetrieveLiveDetails(encoding.Id);
                }
                catch (System.Exception)
                {
                    Thread.Sleep(5000);
                }
            }

            Console.WriteLine("Live stream started");
            Console.WriteLine("Encoding ID: {0}", encoding.Id);
            Console.WriteLine("IP: {0}", liveEncoding.EncoderIp);
            Console.WriteLine("Rtmp URL: rtmp://{0}/live", liveEncoding.EncoderIp);
            Console.WriteLine("Stream Key: {0}", liveEncoding.StreamKey);
        }

        private static Fmp4 CreateFMP4Muxing(Stream stream, BaseOutput output, string outputPath,
            double? segmentLength)
        {
            var encodingOutput = new Encoding.Output
            {
                OutputPath = outputPath,
                OutputId = output.Id,
                Acl = new List<Acl> {new Acl {Permission = Permission.PUBLIC_READ}}
            };

            var muxing = new Fmp4
            {
                Outputs = new List<Encoding.Output> {encodingOutput},
                Streams = new List<MuxingStream> {new MuxingStream {StreamId = stream.Id}},
                SegmentLength = segmentLength
            };

            return muxing;
        }

        private static Stream CreateStream(BaseInput input, string inputPath, int? position,
            CodecConfig codecConfig, SelectionMode selectionMode)
        {

            var inputStream = new InputStream
            {
                InputId = input.Id,
                InputPath = inputPath,
                Position = position,
                SelectionMode = selectionMode
            };

            var stream = new Stream
            {
                InputStreams = new List<InputStream> {inputStream},
                CodecConfigId = codecConfig.Id
            };

            return stream;
        }


    }
}