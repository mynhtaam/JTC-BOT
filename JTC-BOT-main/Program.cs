using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace RadianiterBot
{
    class Program
    {
        // 1. Dán TOKEN Bot của bạn vào đây
        private const string BOT_TOKEN = "MTUzMDEyMTY2NzUyMTY3NTMwNQ.GFfwee.zwkT4hIVu7h5w_Jn7MCP65wNW-DhKMdPv8yJTI";

        // 2. Dán ID của Kênh Voice cố định "➕ Tạo Room" vào đây
        private const ulong JOIN_TO_CREATE_ID = 1530121070642855986;

        // Danh sách lưu các phòng tạm thời do Bot tự tạo
        private static readonly List<ulong> TempChannels = new List<ulong>();

        static async Task Main(string[] args)
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = BOT_TOKEN,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildVoiceStates
            });

            // Đăng ký sự kiện khi trạng thái voice của thành viên thay đổi
            discord.VoiceStateUpdated += OnVoiceStateUpdated;

            await discord.ConnectAsync();
            Console.WriteLine("Bot C# Radianiter đã khởi chạy thành công!");

            // Giữ chương trình chạy liên tục
            await Task.Delay(-1);
        }

        private static async Task OnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            // A. KHI NGƯỜI DÙNG THAM GIA VÀO KÊNH "JOIN TO CREATE"
            if (e.After?.Channel != null && e.After.Channel.Id == JOIN_TO_CREATE_ID)
            {
                var guild = e.Guild;
                var member = e.User as DiscordMember;
                var parentCategory = e.After.Channel.Parent;

                if (member == null) return;

                // Tạo kênh Voice mới
                string channelName = $"🔊 Room của {member.DisplayName}";
                var newChannel = await guild.CreateVoiceChannelAsync(
                    name: channelName,
                    parent: parentCategory
                );

                // Lưu ID phòng mới tạo vào danh sách quản lý
                lock (TempChannels)
                {
                    TempChannels.Add(newChannel.Id);
                }

                // Chuyển người dùng sang phòng mới vừa tạo
                await member.PlaceInAsync(newChannel);
            }

            // B. KHI NGƯỜI DÙNG RỜI KHỎI PHÒNG TẠM THỜI
            if (e.Before?.Channel != null)
            {
                ulong oldChannelId = e.Before.Channel.Id;

                bool isTempChannel;
                lock (TempChannels)
                {
                    isTempChannel = TempChannels.Contains(oldChannelId);
                }

                if (isTempChannel)
                {
                    // Lấy lại thông tin mới nhất của kênh
                    var channel = e.Guild.GetChannel(oldChannelId);

                    // Nếu không còn ai trong phòng -> Xóa phòng
                    if (channel != null && !channel.Users.Any())
                    {
                        lock (TempChannels)
                        {
                            TempChannels.Remove(oldChannelId);
                        }

                        await channel.DeleteAsync("Phòng trống, tự động dọn dẹp.");
                    }
                }
            }
        }
    }
}