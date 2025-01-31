﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using static ZEGO.ZegoConstans;

namespace ZEGO
{
    class ZegoUtil
    {
        public static string PtrToString(IntPtr p)
        {
            if (p == IntPtr.Zero)
                return null;
            return Marshal.PtrToStringAnsi(p);
        }

        public static void GetStructListByPtr<StructType>(ref StructType[] list, IntPtr ptr, uint count)
        {
            for (int i = 0; i < count; ++i)
            {
                IntPtr item_ptr = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(typeof(StructType)) * i);

                if ((list != null) && (item_ptr != null))
                {
                    list[i] = (StructType)Marshal.PtrToStructure(item_ptr, typeof(StructType));
                }
            }
        }
        public static void GetStructListByPtr<StructType>(ref StructType[] list, IntPtr ptr, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                IntPtr item_ptr = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(typeof(StructType)) * i);

                if ((list != null) && (item_ptr != null))
                {
                    list[i] = (StructType)Marshal.PtrToStructure(item_ptr, typeof(StructType));
                }
            }
        }
        public static StructType GetStructByPtr<StructType>(IntPtr ptr)
        {
            return (StructType)Marshal.PtrToStructure(ptr, typeof(StructType));
        }
        //private static ArrayList arrayList = new ArrayList();
        public static IntPtr GetStructPointer(ValueType a)
        {
            int nSize = Marshal.SizeOf(a);                 //定义指针长度
            IntPtr pointer = Marshal.AllocHGlobal(nSize);        //定义指针
            Marshal.StructureToPtr(a, pointer, true);                //将结构体a转为结构体指针
                                                                     // arrayList.Add(pointer);
            return pointer;
        }
        public static IntPtr GetObjPointer(Object obj)
        {
            GCHandle handle = GCHandle.Alloc(obj);
            IntPtr result = (IntPtr)handle;
            handle.Free();
            return result;
        }

        public static void ReleaseAllStructPointers(ArrayList arrayList)
        {
            foreach (System.IntPtr item in arrayList)
            {

                Marshal.FreeHGlobal(item);//释放分配的非托管内存

            }
            arrayList.Clear();

        }
        public static void ReleaseStructPointer(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);//释放分配的非托管内存
        }

        public class UTF8StringMarshaler : ICustomMarshaler//不能修饰结构体，不能解决结构体字符串属性对应utf-8转码问题
        {
            public void CleanUpManagedData(object ManagedObj)
            {
            }

            public void CleanUpNativeData(IntPtr pNativeData)
            {
                Marshal.FreeHGlobal(pNativeData);
            }

            public int GetNativeDataSize()
            {
                return -1;
            }

            public IntPtr MarshalManagedToNative(object ManagedObj)
            {
                if (object.ReferenceEquals(ManagedObj, null)) return IntPtr.Zero;
                if (!(ManagedObj is string)) throw new InvalidOperationException();
                byte[] utf8bytes = Encoding.UTF8.GetBytes(ManagedObj as string);
                IntPtr ptr = Marshal.AllocHGlobal(utf8bytes.Length + 1);
                Marshal.Copy(utf8bytes, 0, ptr, utf8bytes.Length);
                Marshal.WriteByte(ptr, utf8bytes.Length, 0);
                return ptr;
            }

            public object MarshalNativeToManaged(IntPtr pNativeData)
            {
                if (pNativeData == IntPtr.Zero) return null;
                List<byte> bytes = new List<byte>();
                for (int offset = 0; ; offset++)
                {
                    byte b = Marshal.ReadByte(pNativeData, offset);
                    if (b == 0) break;
                    else bytes.Add(b);
                }
                return Encoding.UTF8.GetString(bytes.ToArray(), 0, bytes.Count);
            }
            // 添加以下静态GetInstance方法
            static UTF8StringMarshaler instance = new UTF8StringMarshaler();
            public static ICustomMarshaler GetInstance(string cookie)
            {
                return instance;
            }
            // 以上添加
        }
        public static string GetUTF8String(byte[] data)
        {
            string result = null;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0)
                {
                    result = Encoding.UTF8.GetString(data, 0, i);
                    break;
                }
            }
            return result;
        }
        private static void ZegoLog(string log)
        {
            IExpressPrivateInternal.zego_express_custom_log(log, ZegoConstans.MODULE);
        }

        private static void HandleDebug(int module, int errorCode, string funcName)
        {
            IntPtr detail = IExpressPrivateInternal.zego_express_get_print_debug_info(module, funcName, errorCode);
            if (errorCode != 0)
            {
                IExpressPrivateInternal.zego_express_trigger_on_debug_error(errorCode, funcName, ZegoUtil.PtrToString(detail));
            }
        }
        public static void ZegoPrivateLog(int errorCode, string log, bool handleDebugFlag, int moudle, bool writeFile = true, [CallerMemberName] string funcName = "")
        {
            if (errorCode != 0)
            {
                Console.WriteLine(log);
            }
            if (writeFile)
            {
                ZegoUtil.ZegoLog(log);
            }
            if (handleDebugFlag)
            {
                ZegoUtil.HandleDebug(moudle, errorCode, funcName);
            }
        }

    }

    class ZegoCallBackChangeUtil
    {
        public static T GetCallbackFromSeq<T>(ConcurrentDictionary<int, T> dics, int seq)
        {
            T callbak = default(T);
            dics.TryGetValue(seq, out callbak);
            return callbak;
        }
        public static T GetObjectFromIndex<T>(ConcurrentDictionary<int, T> dics, int index)
        {
            T obj = default(T);
            dics.TryGetValue(index, out obj);
            return obj;
        }
        public static List<ZegoRoomExtraInfo> ChangeZegoRoomExtraInfoStructListToClassList(zego_room_extra_info[] zego_Room_Extra_Infos)
        {

            int length = zego_Room_Extra_Infos.Length;
            List<ZegoRoomExtraInfo> result = new List<ZegoRoomExtraInfo>();
            for (int i = 0; i < length; i++)
            {
                result.Add(ChangeZegoRoomExtraInfoStructToClass(zego_Room_Extra_Infos[i]));
            }
            return result;
        }
        private static ZegoRoomExtraInfo ChangeZegoRoomExtraInfoStructToClass(zego_room_extra_info zego_room_extra_info)
        {
            ZegoRoomExtraInfo zegoRoomExtraInfo = new ZegoRoomExtraInfo();
            zegoRoomExtraInfo.key = ZegoUtil.GetUTF8String(zego_room_extra_info.key);
            zegoRoomExtraInfo.value = ZegoUtil.GetUTF8String(zego_room_extra_info.value);
            zegoRoomExtraInfo.updateTime = zego_room_extra_info.update_time;
            zegoRoomExtraInfo.updateUser = ChangeZegoUserStructToClass(zego_room_extra_info.update_user);
            return zegoRoomExtraInfo;
        }
        public static ZegoUser ChangeZegoUserStructToClass(zego_user user)
        {
            ZegoUser zegoUser = new ZegoUser();
            zegoUser.userID = ZegoUtil.GetUTF8String(user.user_id);
            zegoUser.userName = ZegoUtil.GetUTF8String(user.user_name);
            return zegoUser;
        }

        public static List<ZegoStream> ChangeZegoStreamStructListToClassList(zego_stream[] streams)
        {
            int length = streams.Length;
            List<ZegoStream> zegoStreams = new List<ZegoStream>();
            for (int i = 0; i < length; i++)
            {
                zegoStreams.Add(ChangeZegoStreamStructToClass(streams[i]));
            }
            return zegoStreams;
        }
        private static ZegoStream ChangeZegoStreamStructToClass(zego_stream stream)
        {
            ZegoStream zegoStream = new ZegoStream();
            zegoStream.streamID = ZegoUtil.GetUTF8String(stream.stream_id);
            zegoStream.extraInfo = ZegoUtil.GetUTF8String(stream.extra_info);
            zegoStream.user = ChangeZegoUserStructToClass(stream.user);
            return zegoStream;
        }

        public static ZegoPlayStreamQuality ChangePlayerQualityStructToClass(zego_play_stream_quality quality)
        {
            ZegoPlayStreamQuality playStreamQuality = new ZegoPlayStreamQuality();
            playStreamQuality.videoRecvFPS = quality.video_recv_fps;
            playStreamQuality.videoDejitterFPS = quality.video_dejitter_fps;
            playStreamQuality.videoDecodeFPS = quality.video_decode_fps;
            playStreamQuality.videoRenderFPS = quality.video_render_fps;
            playStreamQuality.videoKBPS = quality.video_kbps;
            playStreamQuality.videoBreakRate = quality.video_break_rate;
            playStreamQuality.audioRecvFPS = quality.audio_recv_fps;
            playStreamQuality.audioDejitterFPS = quality.audio_dejitter_fps;
            playStreamQuality.audioDecodeFPS = quality.audio_decode_fps;
            playStreamQuality.audioRenderFPS = quality.audio_render_fps;
            playStreamQuality.audioKBPS = quality.audio_kbps;
            playStreamQuality.audioBreakRate = quality.audio_break_rate;
            playStreamQuality.rtt = quality.rtt;
            playStreamQuality.packetLostRate = quality.packet_lost_rate;
            playStreamQuality.peerToPeerDelay = quality.peer_to_peer_delay;
            playStreamQuality.peerToPeerPacketLostRate = quality.peer_to_peer_packet_lost_rate;
            playStreamQuality.level = quality.level;
            playStreamQuality.delay = quality.delay;
            playStreamQuality.avTimestampDiff = quality.av_timestamp_diff;
            playStreamQuality.isHardwareDecode = quality.is_hardware_decode;
            playStreamQuality.videoCodecID = quality.video_codec_id;
            playStreamQuality.totalRecvBytes = quality.total_recv_bytes;
            playStreamQuality.audioRecvBytes = quality.audio_recv_bytes;
            playStreamQuality.videoRecvBytes = quality.video_recv_bytes;
            return playStreamQuality;

        }
        public static List<ZegoStreamRelayCDNInfo> ChangeZegoStreamRelayCDNInfoStructListToClassList(zego_stream_relay_cdn_info[] infos)
        {
            List<ZegoStreamRelayCDNInfo> lists = new List<ZegoStreamRelayCDNInfo>();
            for (int i = 0; i < infos.Length; i++)
            {
                lists.Add(ChangeZegoStreamRelayCDNInfoStructToClass(infos[i]));
            }
            return lists;
        }

        private static ZegoStreamRelayCDNInfo ChangeZegoStreamRelayCDNInfoStructToClass(zego_stream_relay_cdn_info zego_stream_relay_cdn_info)
        {
            ZegoStreamRelayCDNInfo zegoStreamRelayCDNInfo = new ZegoStreamRelayCDNInfo();
            zegoStreamRelayCDNInfo.state = zego_stream_relay_cdn_info.state;
            zegoStreamRelayCDNInfo.updateReason = zego_stream_relay_cdn_info.update_reason;
            zegoStreamRelayCDNInfo.stateTime = zego_stream_relay_cdn_info.state_time;
            zegoStreamRelayCDNInfo.url = ZegoUtil.GetUTF8String(zego_stream_relay_cdn_info.url);
            return zegoStreamRelayCDNInfo;
        }


        public static ZegoPublishStreamQuality ChangePublishQualityToClass(zego_publish_stream_quality quality)
        {
            ZegoPublishStreamQuality publishStreamQuality = new ZegoPublishStreamQuality();
            publishStreamQuality.videoCaptureFPS = quality.video_capture_fps;
            publishStreamQuality.videoEncodeFPS = quality.video_encode_fps;
            publishStreamQuality.videoSendFPS = quality.video_send_fps;
            publishStreamQuality.videoKBPS = quality.video_kbps;
            publishStreamQuality.audioCaptureFPS = quality.audio_capture_fps;
            publishStreamQuality.audioSendFPS = quality.audio_send_fps;
            publishStreamQuality.audioKBPS = quality.audio_kbps;
            publishStreamQuality.rtt = quality.rtt;
            publishStreamQuality.packetLostRate = quality.packet_lost_rate;
            publishStreamQuality.level = quality.level;
            publishStreamQuality.isHardwareEncode = quality.is_hardware_encode;
            publishStreamQuality.videoCodecID = quality.video_codec_id;
            publishStreamQuality.totalSendBytes = quality.total_send_bytes;
            publishStreamQuality.audioSendBytes = quality.audio_send_bytes;
            publishStreamQuality.videoSendBytes = quality.video_send_bytes;
            return publishStreamQuality;
        }

        public static float[] GetZegoAudioSpectrumInfoStructSpectrumList(zego_audio_spectrum_info zegoAudioSpectrumInfo)
        {

            float[] results = new float[zegoAudioSpectrumInfo.spectrum_count];
            ZegoUtil.GetStructListByPtr<float>(ref results, zegoAudioSpectrumInfo.spectrum_list, zegoAudioSpectrumInfo.spectrum_count);
            return results;
        }

        public static List<ZegoUser> ChangeZegoUserStructListToClassList(zego_user[] users)
        {
            int length = users.Length;
            List<ZegoUser> zegoUsers = new List<ZegoUser>();
            for (int i = 0; i < length; i++)
            {
                zegoUsers.Add(ChangeZegoUserStructToClass(users[i]));
            }
            return zegoUsers;
        }
        public static List<ZegoBarrageMessageInfo> ChangeBarrageMessageStructListToClassList(zego_barrage_message_info[] infos)
        {
            List<ZegoBarrageMessageInfo> zegoBarrageMessageInfos = new List<ZegoBarrageMessageInfo>();
            for (int i = 0; i < infos.Length; i++)
            {
                zegoBarrageMessageInfos.Add(ChangeBarrageMessageStructToClass(infos[i]));
            }
            return zegoBarrageMessageInfos;

        }

        public static Dictionary<string, float[]> ChangeZegoAudioSpectrumInfoListToDictionary(zego_audio_spectrum_info[] infos)
        {
            Dictionary<string, float[]> keyValuePairs = new Dictionary<string, float[]>();
            for (int i = 0; i < infos.Length; i++)
            {
                string stream_id = ZegoUtil.GetUTF8String(infos[i].stream_id);
                if (keyValuePairs.ContainsKey(stream_id))
                {
                    keyValuePairs[stream_id] = GetZegoAudioSpectrumInfoStructSpectrumList(infos[i]);
                }
                else
                {
                    keyValuePairs.Add(stream_id, GetZegoAudioSpectrumInfoStructSpectrumList(infos[i]));
                }
            }
            return keyValuePairs;
        }
        private static ZegoBarrageMessageInfo ChangeBarrageMessageStructToClass(zego_barrage_message_info zego_barrage_message_info)
        {
            ZegoBarrageMessageInfo zegoBarrageMessageInfo = new ZegoBarrageMessageInfo();
            zegoBarrageMessageInfo.message = ZegoUtil.GetUTF8String(zego_barrage_message_info.message);
            zegoBarrageMessageInfo.messageID = ZegoUtil.GetUTF8String(zego_barrage_message_info.message_id);
            zegoBarrageMessageInfo.sendTime = zego_barrage_message_info.send_time;
            zegoBarrageMessageInfo.fromUser = ChangeZegoUserStructToClass(zego_barrage_message_info.from_user);
            return zegoBarrageMessageInfo;
        }
        public static List<ZegoBroadcastMessageInfo> ChangeBroadMessageStructListToClassList(zego_broadcast_message_info[] infos)
        {
            List<ZegoBroadcastMessageInfo> zegoBroadMessageInfos = new List<ZegoBroadcastMessageInfo>();
            for (int i = 0; i < infos.Length; i++)
            {
                zegoBroadMessageInfos.Add(ChangeBroadMessageStructToClass(infos[i]));
            }
            return zegoBroadMessageInfos;

        }

        private static ZegoBroadcastMessageInfo ChangeBroadMessageStructToClass(zego_broadcast_message_info zego_broad_message_info)
        {
            ZegoBroadcastMessageInfo zegoBroadMessageInfo = new ZegoBroadcastMessageInfo();
            zegoBroadMessageInfo.message = ZegoUtil.GetUTF8String(zego_broad_message_info.message);
            zegoBroadMessageInfo.messageID = zego_broad_message_info.message_id;
            zegoBroadMessageInfo.sendTime = zego_broad_message_info.send_time;
            zegoBroadMessageInfo.fromUser = ChangeZegoUserStructToClass(zego_broad_message_info.from_user);
            return zegoBroadMessageInfo;
        }

        public static Dictionary<string, float> ChangeZegoSoundLevelInfoStructListToDictionary(zego_sound_level_info[] infos)
        {
            Dictionary<string, float> keyValuePairs = new Dictionary<string, float>();
            for (int i = 0; i < infos.Length; i++)
            {
                string stream_id = ZegoUtil.GetUTF8String(infos[i].stream_id);
                if (keyValuePairs.ContainsKey(stream_id))
                {
                    keyValuePairs[stream_id] = infos[i].sound_level;
                }
                else
                {
                    keyValuePairs.Add(stream_id, infos[i].sound_level);
                }
            }
            return keyValuePairs;
        }
        public static Dictionary<uint, float> ChangeZegoMixerSoundLevelInfoStructListToDictionary(zego_mixer_sound_level_info[] infos)
        {
            Dictionary<uint, float> keyValuePairs = new Dictionary<uint, float>();
            for (int i = 0; i < infos.Length; i++)
            {
                if (keyValuePairs.ContainsKey(infos[i].sound_level_id))
                {
                    keyValuePairs[infos[i].sound_level_id] = infos[i].sound_level;
                }
                else
                {
                    keyValuePairs.Add(infos[i].sound_level_id, infos[i].sound_level);
                }
            }
            return keyValuePairs;
        }
        public static ZegoAudioFrameParam ChangeZegoAudioFrameStructToClass(zego_audio_frame_param param)
        {
            ZegoAudioFrameParam zegoAudioFrameParam = new ZegoAudioFrameParam();
            zegoAudioFrameParam.channel = param.channel;
            zegoAudioFrameParam.sampleRate = param.samples_rate;
            return zegoAudioFrameParam;
        }

        public static ZegoVideoFrameParam ChangeZegoVideoFrameStructToClass(zego_video_frame_param param)
        {
            ZegoVideoFrameParam param_ = new ZegoVideoFrameParam();
            param_.format = param.format;
            param_.height = param.height;
            param_.rotation = param.rotation;
            param_.strides = param.strides;
            param_.width = param.width;
            return param_;
        }
        public static ZegoDataRecordConfig ChangeZegoDataRecordConfigStructToClass(zego_data_record_config config)
        {
            return new ZegoDataRecordConfig
            {
                filePath = ZegoUtil.GetUTF8String(config.file_path),
                recordType = config.record_type,
            };
        }

        public static ZegoDataRecordProgress ChangeZegoDataRecordProgresstructToClass(zego_data_record_progress config)
        {
            return new ZegoDataRecordProgress
            {
                currentFileSize = config.current_file_size,
                duration = config.duration,
            };
        }

        public static ZegoVideoFrameParam ChangeZegoVideoFrameParamStructToClass(zego_video_frame_param param)
        {
            ZegoVideoFrameParam zegoVideoFrameParam = new ZegoVideoFrameParam();
            zegoVideoFrameParam.strides = param.strides;
            zegoVideoFrameParam.height = param.height;
            zegoVideoFrameParam.width = param.width;
            zegoVideoFrameParam.rotation = param.rotation;
            zegoVideoFrameParam.format = param.format;
            return zegoVideoFrameParam;
        }

    }

    class ZegoImplCallChangeUtil
    {
        public static ArrayList mixerPtrArrayList;

        public static zego_engine_config ChangeZegoEngineConfigClassToStruct(ZegoEngineConfig config)
        {
            if (config == null)
            {
                throw new Exception("SetEngineConfig param can not be null");
            }
            zego_engine_config engineConfig = new zego_engine_config();
            if (config.logConfig != null)
            {
                zego_log_config logConfig = new zego_log_config();
                if(config.logConfig.logPath != null)
                {
                    logConfig.log_path = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_COMMON_LEN];
                    var bytes = Encoding.UTF8.GetBytes(config.logConfig.logPath);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_COMMON_LEN);
                    Buffer.BlockCopy(bytes, 0, logConfig.log_path, 0, count);
                }
                logConfig.log_size = config.logConfig.logSize;
                engineConfig.log_config = ZegoUtil.GetStructPointer(logConfig);
                Console.WriteLine(string.Format("SetEngineConfig  logConfig.log_path:{0} logConfig.log_size:{1}", logConfig.log_path, logConfig.log_size));
            }
            else
            {
                Console.WriteLine("SetEngineConfig  logConfig is null");
            }
            if (config.advancedConfig != null)
            {
                string advancedConfig = "";
                foreach (KeyValuePair<string, string> item in config.advancedConfig)
                {
                    advancedConfig += item.Key + "=" + item.Value + ";";
                }
                Console.WriteLine(string.Format("SetEngineConfig  advancedConfig:{0}", advancedConfig));
                if(config.logConfig.logPath != null)
                {
                    engineConfig.advanced_config = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_COMMON_LEN];
                    var bytes = Encoding.UTF8.GetBytes(config.logConfig.logPath);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_COMMON_LEN);
                    Buffer.BlockCopy(bytes, 0, engineConfig.advanced_config, 0, count);
                }
            }
            else
            {
                Console.WriteLine("SetEngineConfig  advancedConfig is null");
            }
            return engineConfig;

        }

        public static zego_log_config ChangeZegoLogConfigClassToStruct(ZegoLogConfig config)
        {
            zego_log_config log_config = new zego_log_config();

            if(config != null)
            {
                if(config.logPath != null)
                {
                    log_config.log_path = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_COMMON_LEN];
                    var bytes = Encoding.UTF8.GetBytes(config.logPath);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_COMMON_LEN);
                    Buffer.BlockCopy(bytes, 0, log_config.log_path, 0, count);
                }
                
                log_config.log_size = config.logSize;
            }

            return log_config;
        }

        public static ZegoVideoConfig ChangeVideoConfigStructToClass(zego_video_config zego_Video_Config)
        {
            ZegoVideoConfig zegoVideoConfig = new ZegoVideoConfig();
            zegoVideoConfig.bitrate = zego_Video_Config.bitrate;
            zegoVideoConfig.captureHeight = zego_Video_Config.capture_height;
            zegoVideoConfig.captureWidth = zego_Video_Config.capture_width;
            zegoVideoConfig.encodeHeight = zego_Video_Config.encode_height;
            zegoVideoConfig.encodeWidth = zego_Video_Config.encode_width;
            zegoVideoConfig.codecID = zego_Video_Config.codec_id;
            zegoVideoConfig.fps = zego_Video_Config.fps;
            zegoVideoConfig.keyFrameInterval = zego_Video_Config.key_frame_interval;
            return zegoVideoConfig;
        }

        public static ZegoAudioConfig ChangeAudioConfigStructToClass(zego_audio_config zego_Audio_Config)
        {
            ZegoAudioConfig zegoAudioConfig = new ZegoAudioConfig();
            zegoAudioConfig.bitrate = zego_Audio_Config.bitrate;
            zegoAudioConfig.channel = zego_Audio_Config.channel;
            zegoAudioConfig.codecID = zego_Audio_Config.codec_id;
            return zegoAudioConfig;
        }

        public static zego_video_frame_param ChangeZegoVideoFrameParamClassToStruct(ZegoVideoFrameParam param)
        {
            zego_video_frame_param result = new zego_video_frame_param();
            if (param != null)
            {
                result.format = param.format;
                result.height = param.height;
                result.width = param.width;
                result.rotation = param.rotation;
                result.strides = param.strides;
            }
            return result;
        }

        public static zego_custom_video_capture_config ChangeCustomVideoCaptureConfigClassToStruct(ZegoCustomVideoCaptureConfig config)
        {
            zego_custom_video_capture_config result = new zego_custom_video_capture_config();
            if (config != null)
            {
                result.buffer_type = config.bufferType;
            }
            return result;
        }
        public static IntPtr ChangeZegoRoomConfigClassToStructPoniter(ZegoRoomConfig config)
        {
            if (config == null)
            {
                return IntPtr.Zero;
            }
            else
            {
                zego_room_config roomConfig = new zego_room_config();
                roomConfig.max_member_count = config.maxMemberCount;
                if(config.token != null)
                {
                    roomConfig.thrid_token = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_COMMON_LEN];
                    var bytes = Encoding.UTF8.GetBytes(config.token);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_COMMON_LEN);
                    Buffer.BlockCopy(bytes, 0, roomConfig.thrid_token, 0, count);
                }

                roomConfig.is_user_status_notify = config.isUserStatusNotify;
                Console.WriteLine(string.Format("LoginRoom ZegoRoomConfig max_member_count:{0} thrid_token:{1} is_user_status_notify:{2}", roomConfig.max_member_count, roomConfig.thrid_token, roomConfig.is_user_status_notify));
                return ZegoUtil.GetStructPointer(roomConfig);
            }
        }
        public static zego_user ChangeZegoUserClassToStruct(ZegoUser user)
        {
            zego_user zegoUser;
            if (user == null)
            {
                throw new Exception("ZegoUser should not be null");
            }
            else
            {
                if (user.userID == null)
                {
                    throw new Exception("ZegoUser userId should not be null");
                }
                if (user.userName == null)
                {
                    throw new Exception("ZegoUser userName should not be null");
                }
                zegoUser = new zego_user();
                zegoUser.user_id = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_USERID_LEN];
                zegoUser.user_name = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_USERNAME_LEN];
                var bytes_user_id = Encoding.UTF8.GetBytes(user.userID);
                var bytes_user_name = Encoding.UTF8.GetBytes(user.userName);
                int count_user_id = Math.Min(bytes_user_id.Length, ZEGO_EXPRESS_MAX_USERID_LEN);
                int count_user_name = Math.Min(bytes_user_name.Length, ZEGO_EXPRESS_MAX_USERNAME_LEN);
                Buffer.BlockCopy(bytes_user_id, 0, zegoUser.user_id, 0, count_user_id);
                Buffer.BlockCopy(bytes_user_name, 0, zegoUser.user_name, 0, count_user_name);
            }
            return zegoUser;
        }

        public static zego_engine_profile ChangeZegoEngineProfileClassToStruct(ZegoEngineProfile profile)
        {
            zego_engine_profile engine_profile;
            if (profile == null)
            {
                throw new Exception("ZegoEngineProfile should not be null");
            }
            else
            {
                if (profile.appID == 0)
                {
                    throw new Exception("ZegoEngineProfile appID should not be 0");
                }
                if (profile.appSign == null)
                {
                    throw new Exception("ZegoEngineProfile appSign should not be null");
                }
                engine_profile = new zego_engine_profile();
                engine_profile.app_id = profile.appID;
                engine_profile.app_sign = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_APPSIGN_LEN];
                var bytes = Encoding.UTF8.GetBytes(profile.appSign);
                int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_APPSIGN_LEN);
                Buffer.BlockCopy(bytes, 0, engine_profile.app_sign, 0, count);
                engine_profile.scenario = profile.scenario;
            }
            return engine_profile;
        }

        public static zego_user[] ChangeZegoUserClassListToStructList(List<ZegoUser> toUserList)
        {
            zego_user[] zegoUsers;
            if (toUserList == null)
            {
                throw new Exception("SendCustomCommand toUserList should not be null");
            }
            else
            {
                zegoUsers = new zego_user[toUserList.Count];
                for (int i = 0; i < toUserList.Count; i++)
                {
                    zegoUsers[i] = ChangeZegoUserClassToStruct(toUserList[i]);
                }
            }
            return zegoUsers;
        }
        public static ZegoDeviceInfo[] ChangeZegoDeviceInfoStructListToClassList(zego_device_info[] zego_Device_Info)
        {
            ZegoDeviceInfo[] result = null;
            if (zego_Device_Info != null)
            {
                result = new ZegoDeviceInfo[zego_Device_Info.Length];
                for (int i = 0; i < zego_Device_Info.Length; i++)
                {
                    ZegoDeviceInfo zegoDevice = new ZegoDeviceInfo();
                    zegoDevice.deviceID = ZegoUtil.GetUTF8String(zego_Device_Info[i].device_id);
                    zegoDevice.deviceName = ZegoUtil.GetUTF8String(zego_Device_Info[i].device_name);
                    result[i] = zegoDevice;
                }
            }
            return result;
        }

        public static zego_player_config ChangeZegoPlayerConfigClassToStruct(ZegoPlayerConfig config)
        {
            zego_player_config zegoPlayerConfig;
            if (config == null)
            {
                throw new Exception("ZegoPlayerConfig should not be null");
            }
            else
            {
                zegoPlayerConfig = new zego_player_config();
                zegoPlayerConfig.resource_mode = config.resourceMode;
                zegoPlayerConfig.cdn_config = ZegoUtil.GetStructPointer(ChangeCDNConfigClassToStruct(config.cdnConfig));
                zegoPlayerConfig.video_layer = config.videoLayer;
                if(config.roomID != null)
                {
                    zegoPlayerConfig.room_id = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_ROOMID_LEN];
                    var bytes = Encoding.UTF8.GetBytes(config.roomID);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_ROOMID_LEN);
                    Buffer.BlockCopy(bytes, 0, zegoPlayerConfig.room_id, 0, count);
                }
                //Console.WriteLine(string.Format("StartPlayingStream ZegoPlayerConfig url:{0} authParam:{1} video_layer{2}", config.cdnConfig.url, config.cdnConfig.authParam, config.videoLayer));
                return zegoPlayerConfig;
            }

        }

        public static zego_cdn_config ChangeCDNConfigClassToStruct(ZegoCDNConfig cDNConfig)
        {
            zego_cdn_config config = new zego_cdn_config();
            if (cDNConfig != null)
            {
                if(cDNConfig.authParam != null)
                {
                    config.auth_param = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_COMMON_LEN];
                    var bytes = Encoding.UTF8.GetBytes(cDNConfig.authParam);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_COMMON_LEN);
                    Buffer.BlockCopy(bytes, 0, config.auth_param, 0, count);
                }
                if (cDNConfig.url != null)
                {
                    config.url = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_URL_LEN];
                    var bytes = Encoding.UTF8.GetBytes(cDNConfig.url);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_URL_LEN);
                    Buffer.BlockCopy(bytes, 0, config.url, 0, count);
                }
            }
            return config;
        }

        public static zego_video_config ChangeVideoConfigClassToStruct(ZegoVideoConfig config)
        {
            zego_video_config zegoVideoConfig;
            if (config == null)
            {
                throw new Exception("ZegoVideoConfig should not be null");
            }
            else
            {
                zegoVideoConfig = new zego_video_config();
                zegoVideoConfig.capture_width = config.captureWidth;
                zegoVideoConfig.capture_height = config.captureHeight;
                zegoVideoConfig.encode_width = config.encodeWidth;
                zegoVideoConfig.encode_height = config.encodeHeight;
                zegoVideoConfig.bitrate = config.bitrate;
                zegoVideoConfig.fps = config.fps;
                zegoVideoConfig.codec_id = config.codecID;
                zegoVideoConfig.key_frame_interval = config.keyFrameInterval;
            }
            return zegoVideoConfig;
        }

        public static zego_audio_config ChangeAudioConfigClassToStruct(ZegoAudioConfig config)
        {
            zego_audio_config zegoAudioConfig;
            if (config == null)
            {
                throw new Exception("ZegoAudioConfig should not be null");
            }
            else
            {
                zegoAudioConfig.bitrate = config.bitrate;
                zegoAudioConfig.channel = config.channel;
                zegoAudioConfig.codec_id = config.codecID;
            }
            return zegoAudioConfig;
        }

        public static zego_beautify_option ChangeZegoBeautifyOptionToStruct(ZegoBeautifyOption option)
        {
            zego_beautify_option option1 = new zego_beautify_option();
            if (option == null)
            {
                throw new Exception("ZegoBeautifyOption should not be null");
            }
            else
            {
                option1.polish_step = option.polishStep;
                option1.sharpen_factor = option.sharpenFactor;
                option1.whiten_factor = option.whitenFactor;
                return option1;
            }
        }
        public static zego_watermark ChangeWaterMarkClassToStruct(ZegoWatermark watermark)
        {
            if (watermark == null)
            {
                throw new Exception("ZegoWatermark should not be null");
            }
            else
            {
                zego_watermark zegoWatermark = new zego_watermark();
                if(watermark.imageURL != null)
                {
                    zegoWatermark.image = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_COMMON_LEN];
                    var bytes = Encoding.UTF8.GetBytes(watermark.imageURL);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_COMMON_LEN);
                    Buffer.BlockCopy(bytes, 0, zegoWatermark.image, 0, count);
                }
                zegoWatermark.layout = ChangeRectClassToStruct(watermark.layout);
                return zegoWatermark;
            }
        }

        public static zego_rect ChangeRectClassToStruct(ZegoRect layout)
        {
            if (layout == null)
            {
                throw new Exception("ZegoRect in ZegoWatermark should not be null");
            }
            else
            {
                zego_rect rect = new zego_rect();
                rect.top = layout.y;
                rect.bottom = layout.y + layout.height;
                rect.left = layout.x;
                rect.right = layout.x + layout.width;
                return rect;
            }
        }
        public static zego_mixer_task ChangeZegoMixerTaskClassToStruct(ZegoMixerTask task)
        {
            mixerPtrArrayList = new ArrayList();
            zego_mixer_task result = new zego_mixer_task();
            if (task == null)
            {
                throw new Exception("ZegoMixerTask Class should not be null");
            }
            else
            {
                if(task.taskID != null)
                {
                    result.task_id = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_MIXER_TASK_LEN];
                    var bytes = Encoding.UTF8.GetBytes(task.taskID);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_MIXER_TASK_LEN);
                    Buffer.BlockCopy(bytes, 0, result.task_id, 0, count);
                }
                zego_mixer_input[] zego_Mixer_Inputs = ChangeZegoMixerInputClassListToStructList(task.inputList);
                result.input_list_count = (uint)zego_Mixer_Inputs.Length;
                IntPtr inputListPtr = GetMixerInputListPtr(zego_Mixer_Inputs);
                mixerPtrArrayList.Add(inputListPtr);
                result.input_list = inputListPtr;

                foreach(var output_config in task.outputList)
                {
                    if(output_config.videoConfig.bitrate <= 0)
                    {
                        output_config.videoConfig.bitrate = task.videoConfig.bitrate;
                    }
                }

                zego_mixer_output[] zego_Mixer_Outputs = ChangeZegoMixerOutputClassListToStructList(task.outputList);
                result.output_list_count = (uint)zego_Mixer_Outputs.Length;

                foreach(var x in zego_Mixer_Outputs)
                {
                    mixerPtrArrayList.Add(x.video_config);
                }
                IntPtr outPutListPtr = GetMixerOutputListPtr(zego_Mixer_Outputs);
                mixerPtrArrayList.Add(outPutListPtr);
                result.output_list = outPutListPtr;
                result.audio_config = ChangeZegoMixerAudioConfigClassToStruct(task.audioConfig);
                result.video_config = ChangeZegoMixerVideoConfigClassToStruct(task.videoConfig);
                if (task.watermark != null)
                {
                    result.watermark = ZegoUtil.GetStructPointer(ChangeWaterMarkClassToStruct(task.watermark));
                }
                result.background_color = task.backgroundColor;
                if(task.backgroundImageURL != null)
                {
                    result.background_image_url = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_URL_LEN];
                    var bytes = Encoding.UTF8.GetBytes(task.backgroundImageURL);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_URL_LEN);
                    Buffer.BlockCopy(bytes, 0, result.background_image_url, 0, ZEGO_EXPRESS_MAX_URL_LEN);
                }
                result.enable_sound_level = task.enableSoundLevel;
                result.stream_alignment_mode = task.streamAlignmentMode;
                result.user_data = task.userData;
                result.user_data_length = task.userDataLength;
                string advanceConfig = null;
                foreach (var iter in task.advancedConfig)
                {
                    advanceConfig += iter.Key + "=" + iter.Value + ";";
                }
                if(advanceConfig != null)
                {
                    result.advanced_config = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_COMMON_LEN];
                    var bytes = Encoding.UTF8.GetBytes(advanceConfig);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_COMMON_LEN);
                    Buffer.BlockCopy(bytes, 0, result.advanced_config, 0, count);
                }
            }
            return result;
        }

        public static IntPtr GetMixerOutputListPtr(zego_mixer_output[] zego_Mixer_Outputs)
        {
            int size = Marshal.SizeOf(typeof(zego_mixer_output));
            IntPtr result = Marshal.AllocHGlobal(zego_Mixer_Outputs.Length * size);
            long LongPtr = result.ToInt64();
            for (int i = 0; i < zego_Mixer_Outputs.Length; i++)
            {
                IntPtr a = new IntPtr(LongPtr);
                Marshal.StructureToPtr(zego_Mixer_Outputs[i], a, true);
                LongPtr += Marshal.SizeOf(typeof(zego_mixer_output));
            }
            return result;
        }

        public static IntPtr GetMixerInputListPtr(zego_mixer_input[] zego_Mixer_Inputs)
        {
            int size = Marshal.SizeOf(typeof(zego_mixer_input));
            IntPtr result = Marshal.AllocHGlobal(zego_Mixer_Inputs.Length * size);
            long LongPtr = result.ToInt64();
            for (int i = 0; i < zego_Mixer_Inputs.Length; i++)
            {
                IntPtr a = new IntPtr(LongPtr);
                Marshal.StructureToPtr(zego_Mixer_Inputs[i], a, true);
                LongPtr += Marshal.SizeOf(typeof(zego_mixer_input));
            }
            return result;
        }


        public static zego_mixer_video_config ChangeZegoMixerVideoConfigClassToStruct(ZegoMixerVideoConfig videoConfig)
        {
            zego_mixer_video_config config = new zego_mixer_video_config();
            if (videoConfig != null)
            {
                config.bitrate = videoConfig.bitrate;
                config.fps = videoConfig.fps;
                config.height = videoConfig.height;
                config.width = videoConfig.width;
            }
            return config;
        }

        public static zego_mixer_audio_config ChangeZegoMixerAudioConfigClassToStruct(ZegoMixerAudioConfig audioConfig)
        {
            zego_mixer_audio_config config = new zego_mixer_audio_config();
            if (audioConfig != null)
            {
                config.codec_id = audioConfig.codecID;
                config.channel = audioConfig.channel;
                config.bitrate = audioConfig.bitrate;
                config.mix_mode = audioConfig.mixMode;
            }
            return config;
        }

        public static zego_mixer_input[] ChangeZegoMixerInputClassListToStructList(List<ZegoMixerInput> inputList)
        {
            zego_mixer_input[] result = new zego_mixer_input[inputList.Count];
            if (inputList == null)
            {
                throw new Exception("List<ZegoMixerInput> should not be null");
            }
            else
            {
                for (int i = 0; i < inputList.Count; i++)
                {
                    zego_mixer_input zegoMixerInput = ChangeZegoMixerInputClassToStruct(inputList[i]);
                    result[i] = zegoMixerInput;
                }
                return result;

            }
        }

        public static zego_mixer_input ChangeZegoMixerInputClassToStruct(ZegoMixerInput zegoMixerInput)
        {
            zego_mixer_input zego_Mixer_Input = new zego_mixer_input();
            if (zegoMixerInput != null)
            {
                zego_Mixer_Input.content_type = zegoMixerInput.contentType;
                zego_Mixer_Input.sound_level_id = zegoMixerInput.soundLevelID;
                if(zegoMixerInput.streamID != null)
                {
                    zego_Mixer_Input.stream_id = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_STREAM_LEN];
                    var bytes = Encoding.UTF8.GetBytes(zegoMixerInput.streamID);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_STREAM_LEN);
                    Buffer.BlockCopy(bytes, 0, zego_Mixer_Input.stream_id, 0, count);
                }
                zego_Mixer_Input.layout = ChangeRectClassToStruct(zegoMixerInput.layout);
                zego_Mixer_Input.is_audio_focus = zegoMixerInput.isAudioFocus;
                zego_Mixer_Input.audio_direction = zegoMixerInput.audioDirection;
                zego_Mixer_Input.label.font.color = zegoMixerInput.label.font.color;
                zego_Mixer_Input.label.font.size = zegoMixerInput.label.font.size;
                zego_Mixer_Input.label.font.transparency = zegoMixerInput.label.font.transparency;
                zego_Mixer_Input.label.font.type = zegoMixerInput.label.font.type;
                zego_Mixer_Input.label.left = zegoMixerInput.label.left;
                if (zegoMixerInput.label.text != null)
                {
                    zego_Mixer_Input.label.text = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_COMMON_LEN];
                    var bytes = Encoding.UTF8.GetBytes(zegoMixerInput.label.text);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_COMMON_LEN);
                    Buffer.BlockCopy(bytes, 0, zego_Mixer_Input.label.text, 0, count);
                }
                zego_Mixer_Input.label.top = zegoMixerInput.label.top;
                zego_Mixer_Input.render_mode = zegoMixerInput.renderMode;
                if(zego_Mixer_Input.audio_direction < 0)
                {
                    zego_Mixer_Input.enable_audio_direction = false;
                }
                else
                {
                    zego_Mixer_Input.enable_audio_direction = true;
                }
            }
            return zego_Mixer_Input;
        }
        public static zego_mixer_output[] ChangeZegoMixerOutputClassListToStructList(List<ZegoMixerOutput> outputList)
        {
            zego_mixer_output[] result = new zego_mixer_output[outputList.Count];
            if (outputList == null)
            {
                throw new Exception("List<ZegoMixerOutput> should not be null");
            }
            else
            {
                for (int i = 0; i < outputList.Count; i++)
                {
                    zego_mixer_output zegoMixerOutput = ChangeZegoMixerOutputClassToStruct(outputList[i]);
                    result[i] = zegoMixerOutput;
                }
                return result;

            }
        }

        public static zego_mixer_output ChangeZegoMixerOutputClassToStruct(ZegoMixerOutput zegoMixerOutput)
        {
            zego_mixer_output zego_Mixer_Output = new zego_mixer_output();
            if(zegoMixerOutput.target != null)
            {
                zego_Mixer_Output.target = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_URL_LEN];
                var bytes = Encoding.UTF8.GetBytes(zegoMixerOutput.target);
                int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_COMMON_LEN);
                Buffer.BlockCopy(bytes, 0, zego_Mixer_Output.target, 0, count);
            }
            zego_mixer_output_video_config video_config = new zego_mixer_output_video_config();
            video_config.bitrate = zegoMixerOutput.videoConfig.bitrate;
            video_config.encode_latency = zegoMixerOutput.videoConfig.encodeLatency;
            video_config.encode_profile = zegoMixerOutput.videoConfig.encodeProfile;
            video_config.video_codec_id = zegoMixerOutput.videoConfig.videoCodecID;
            IntPtr video_config_ptr = ZegoUtil.GetStructPointer(video_config);
            zego_Mixer_Output.video_config = video_config_ptr;
            return zego_Mixer_Output;
        }

        public static int GetIndexFromObject<T>(ConcurrentDictionary<int, T> dics, T obj)
        {
            int index = -1;
            foreach (KeyValuePair<int, T> kvp in dics)
            {
                if (EqualityComparer<T>.Default.Equals(kvp.Value, obj))
                {
                    index = kvp.Key;
                }
            }

            return index;
        }
        public static zego_audio_effect_play_config ChangeZegoAudioEffectPlayConfigClassToStruct(ZegoAudioEffectPlayConfig config)
        {
            zego_audio_effect_play_config zego_Audio_Effect_Play_Config = new zego_audio_effect_play_config();
            if (config != null)
            {
                zego_Audio_Effect_Play_Config.is_publish_out = config.isPublishOut;
                zego_Audio_Effect_Play_Config.play_count = config.playCount;
            }
            return zego_Audio_Effect_Play_Config;
        }

        public static zego_audio_frame_param ChangeZegoAudioFrameParamClassToStruct(ZegoAudioFrameParam param)
        {
            zego_audio_frame_param result = new zego_audio_frame_param();
            if (param == null)
            {
                throw new Exception("EnableAudioDataCallback ZegoAudioFrameParam should not be null");
            }
            else
            {
                result.channel = param.channel;
                result.samples_rate = param.sampleRate;
            }
            return result;
        }
        public static zego_custom_audio_config ChangeZegoCustomAudioConfigClassToStruct(ZegoCustomAudioConfig config)
        {
            zego_custom_audio_config result = new zego_custom_audio_config();
            if (config == null)
            {
                throw new Exception("EnableCustomAudioIO ZegoCustomAudioConfig should not be null");
            }
            else
            {
                result.source_type = config.sourceType;
            }
            return result;
        }
        public static zego_data_record_config ChangeZegoDataRecordConfigClassToStruct(ZegoDataRecordConfig config)
        {
            var result = new zego_data_record_config();
            if (config != null)
            {
                if(config.filePath != null)
                {
                    result.file_path = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_URL_LEN];
                    var bytes = Encoding.UTF8.GetBytes(config.filePath);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_URL_LEN);
                    Buffer.BlockCopy(bytes, 0, result.file_path, 0, count);
                }
                result.record_type = config.recordType;
            }
            return result;
        }

        //public static zego_custom_video_process_config ChangeZegoCustomVideoProcessConfigToStruct(ZegoCustomVideoProcessConfig config)
        //{
        //    var result = new zego_custom_video_process_config();
        //    if (config != null)
        //    {
        //        result.buffer_type = config.bufferType;
        //    }
        //    return result;
        //}
        public static zego_custom_video_process_config ChangeZegoCustomVideoProcessConfigToStruct(ZegoCustomVideoProcessConfig config)
        {
            var result = new zego_custom_video_process_config();
            if (config != null)
            {
                result.buffer_type = config.bufferType;
            }
            return result;
        }

        public static zego_sound_level_config ChangeZegoSoundLevelConfigToStruct(ZegoSoundLevelConfig config)
        {
            var result = new zego_sound_level_config();
            if (config != null)
            {
                result.enable_vad = config.enableVAD;
                result.millisecond = config.millisecond;
            }
            return result;
        }

        public static zego_publisher_config ChangeZegoPublisherConfigToStruct(ZegoPublisherConfig config)
        {
            var result = new zego_publisher_config();
            if (config != null)
            {
                if(config.roomID != null)
                {
                    result.room_id = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_ROOMID_LEN];
                    var bytes = Encoding.UTF8.GetBytes(config.roomID);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_ROOMID_LEN);
                    Buffer.BlockCopy(bytes, 0, result.room_id, 0, count);
                }
                //result.force_synchronous_network_time//TODO
            }
            return result;
        }

        public static zego_custom_video_render_config ChangeZegoCustomVideoRenderConfigClassToStruct(ZegoCustomVideoRenderConfig config)
        {
            zego_custom_video_render_config customVideoRenderConfig = new zego_custom_video_render_config();
            if (config != null)
            {
                customVideoRenderConfig.type = config.bufferType;
                customVideoRenderConfig.series = config.frameFormatSeries;
                customVideoRenderConfig.is_internal_render = config.enableEngineRender;
            }
            return customVideoRenderConfig;
        }

        public static zego_canvas ChangeZegoCanvasClassToStruct(ZegoCanvas zegoCanvas)
        {
            if (zegoCanvas == null)
            {
                throw new Exception("ZegoCanvas should not be null");
            }
            else
            {
                zego_canvas result = new zego_canvas();
                result.background_color = zegoCanvas.backgroundColor;
                result.view = zegoCanvas.view;
                result.view_mode = zegoCanvas.viewMode;
                return result;
            }
        }

        public static zego_copyrighted_music_config ChangeZegoCopyrightedMusicConfigClassToStruct(ZegoCopyrightedMusicConfig config)
        {
            zego_copyrighted_music_config music_config = new zego_copyrighted_music_config();

            if (config != null)
            {
                if (config.user != null)
                { 
                    if(config.user.userID!= null)
                    {
                        music_config.user.user_id = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_USERID_LEN];
                        var bytes = Encoding.UTF8.GetBytes(config.user.userID);
                        int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_USERID_LEN);
                        Buffer.BlockCopy(bytes, 0, music_config.user.user_id, 0, count);
                    }
                    if(config.user.userName != null)
                    {
                        music_config.user.user_name = new byte[ZegoConstans.ZEGO_EXPRESS_MAX_USERNAME_LEN];
                        var bytes = Encoding.UTF8.GetBytes(config.user.userName);
                        int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_USERNAME_LEN);
                        Buffer.BlockCopy(bytes, 0, music_config.user.user_name, 0, count);
                    }
                }
                //music_config
            }

            return music_config;
        }

        public static zego_copyrighted_music_request_config ChangeZegoCopyrightedMusicRequestConfigClassToStruct(ZegoCopyrightedMusicRequestConfig config)
        {
            zego_copyrighted_music_request_config request_config = new zego_copyrighted_music_request_config();

            if(config != null)
            {
                request_config.mode = (zego_copyrighted_music_billing_mode)config.mode;
                if(config.songID != null)
                {
                    request_config.song_id = new byte[ZEGO_EXPRESS_MAX_COMMON_LEN];
                    var bytes = Encoding.UTF8.GetBytes(config.songID);
                    int count = Math.Min(bytes.Length, ZEGO_EXPRESS_MAX_COMMON_LEN);
                    Buffer.BlockCopy(bytes, 0, request_config.song_id, 0, count);
                }
            }

            return request_config;
        }

    }
}

