document.addEventListener("DOMContentLoaded", function () {

    const hocVienData = [
        { anh: "User.jpg", maHocVIen: "2001215669", tenHocVien: "Bùi Khánh Duy", email: "buikhanhduy13082003@gmail.com"},
    ]

    const thongTinHocVien = document.getElementById("thongTinHocVien");

    hocVienData.forEach(hocvien => {
        const row = document.createElement("tr");

        const subjectCell = document.createElement("td");
        subjectCell.textContent = hocvien.anh;
        row.appendChild(subjectCell);

        const timeCell = document.createElement("td");
        timeCell.textContent = hocvien.maHocVIen;
        row.appendChild(timeCell);

        const roomCell = document.createElement("td");
        roomCell.textContent = hocvien.tenHocVien;
        row.appendChild(roomCell);

        const emailCell = document.createElement("td");
        emailCell.textContent = hocvien.email;
        row.appendChild(emailCell);

        thongTinHocVien.appendChild(row);
    });

    const scheduleData = [
        { subject: "Lập trình Java", time: "Thứ 2 - 08:00 - 10:00", room: "Phòng A101" },
        { subject: "Cơ sở dữ liệu", time: "Thứ 4 - 10:00 - 12:00", room: "Phòng B203" },
        { subject: "Lập trình web", time: "Thứ 6 - 13:00 - 15:00", room: "Phòng C105" }
    ];

    const scheduleTable = document.getElementById("scheduleTable");

    scheduleData.forEach(schedule => {
        const row = document.createElement("tr");

        const subjectCell = document.createElement("td");
        subjectCell.textContent = schedule.subject;
        row.appendChild(subjectCell);

        const timeCell = document.createElement("td");
        timeCell.textContent = schedule.time;
        row.appendChild(timeCell);

        const roomCell = document.createElement("td");
        roomCell.textContent = schedule.room;
        row.appendChild(roomCell);

        scheduleTable.appendChild(row);
    });
});