StaffLevelStatus = {
    0: "critical",
    1: "decent",
    2: "acceptable",    
    critical: 0,
    decent: 1,
    acceptable: 2    
};

LeaveRequestStatus = {
    0: "Pending",
    1: "Accepted",
    2: "Rejected",
    pending: 0,
    accepted: 1,
    rejected: 2
};

Frequency = {
    0: "3 Months",
    1: "6 Months",
    2: "12 Months",
    ThreeMonths: 0,
    SixMonths: 1,
    TwelveMonths: 2
}

const TIMEOUT_DURATION = 15 * Math.pow(10, 3); 
const MODE_EDIT = "edit";
const MODE_CREATE = "create";