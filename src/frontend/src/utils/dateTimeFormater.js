import format from 'date-fns/format';
import isWithinInterval from 'date-fns/isWithinInterval';
import moment from 'moment';

export const parseDate = dateString => {
    if (!dateString) return null;
    try {
        const parts = dateString.split('.');
        let d = new Date(parts[2], parts[1] - 1, parts[0]);
        return isNaN(d.getTime()) ? null : d;
    } catch (e) {
        return null;
    }
};

export const parseDateTime = dateString => {
    if (!dateString) return null;
    let reg = /(\d{2}).(\d{2}).(\d{4}) (\d{2}):(\d{2})/;
    let dateArray = reg.exec(dateString);
    if (!dateArray) return null;
    let d = new Date(+dateArray[3], +dateArray[2] - 1, +dateArray[1], +dateArray[4], +dateArray[5]);
    return isNaN(d.getTime()) ? null : d;
};

export const dateToString = (date = new Date(), dateFormat = 'dd.MM.yyyy') => {
    return format(date, dateFormat);
};

export const formatDate = (date, dateFormat = 'dd.MM.yyyy') => {
    if (!date) return null;

    return format(date, dateFormat);
};

export const dateToUTC = (date, format = 'DD.MM.YYYY', isNotUtc) => {
    if (!date) {
        return null;
    }

    if (isNotUtc) {
        return moment(date).format(format);
    }

    const stillUtc = moment.utc(date).toDate();
    const local = moment(stillUtc)
        .local()
        .format(format);

    return local;
};

export const isRange = (dateStart, dateEnd, date) => {
    return isWithinInterval(parseDate(date), {
        start: parseDate(dateStart),
        end: dateEnd ? parseDate(dateEnd) : new Date(),
    });
};
