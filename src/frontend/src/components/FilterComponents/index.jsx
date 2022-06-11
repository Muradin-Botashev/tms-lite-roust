import React, { useRef, useEffect, useState, forwardRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Icon, Popup } from 'semantic-ui-react';
import AwesomeDebouncePromise from 'awesome-debounce-promise';
import './style.scss';
import {
    BIG_TEXT_TYPE,
    BOOLEAN_TYPE,
    DATE_TIME_TYPE,
    DATE_TYPE,
    ENUM_TYPE,
    LINK_TYPE,
    NUMBER_TYPE,
    VALIDATED_NUMBER_TYPE,
    SELECT_TYPE,
    STATE_TYPE,
    TEXT_TYPE,
    TIME_TYPE,
    LOCAL_DATE_TIME,
    ACTIVE_TYPE,
    INTEGER_TYPE, CUSTOM_STATE_TYPE, MULTI_ENUM_TYPE, MULTI_SELECT_TYPE,
} from '../../constants/columnTypes';
import TextFacet from './Text';
import NumberFacet from './Number';
import SelectFacet from './Select';
import DateFacet from './Date';
import TimeFacet from './Time';
import StateFacet from './State';
import Bool from './Bool';
import TextCropping from './TextCropping';

const getTypeFacet = {
    [TEXT_TYPE]: <TextFacet />,
    [BIG_TEXT_TYPE]: <TextFacet />,
    [NUMBER_TYPE]: <NumberFacet />,
    [VALIDATED_NUMBER_TYPE]: <NumberFacet />,
    [SELECT_TYPE]: <SelectFacet />,
    [MULTI_ENUM_TYPE]: <SelectFacet />,
    [MULTI_SELECT_TYPE]: <SelectFacet />,
    [SELECT_TYPE]: <SelectFacet />,
    [DATE_TIME_TYPE]: <DateFacet />,
    [DATE_TYPE]: <DateFacet />,
    [LOCAL_DATE_TIME]: <DateFacet />,
    [TIME_TYPE]: <TimeFacet />,
    [STATE_TYPE]: <StateFacet />,
    [BOOLEAN_TYPE]: <Bool />,
    [ENUM_TYPE]: <SelectFacet />,
    [LINK_TYPE]: <TextFacet />,
    [ACTIVE_TYPE]: <Bool />,
    [INTEGER_TYPE]: <TextFacet />,
};

const searchDebounced = AwesomeDebouncePromise((search) => search(), 1000);

const Control = forwardRef((props, ref) => {
    const { type } = props;

    return React.cloneElement(getTypeFacet[type] || <TextFacet />, {...props, ref});
});

const FacetField = ({
    name,
    sort,
    setSort,
    type,
    value: propsValue,
    setFilter,
    source,
    index,
    handleResize,
    width,
    displayNameKey,
    notWrapHeader,
    notSortAndFilter,
    gridName,
    filters,
    noSort,
                        emptyValue,
}) => {
    const { t } = useTranslation();
    let [value, setValue] = useState(propsValue);

    useEffect(() => {
        setValue(propsValue);
    }, [propsValue]);

    const handleChangeFilter = async (e, data) => {
        setValue(data.value);
        await searchDebounced(() => setFilter(e, data));
    };

    const handleSort = () => {
        if (sort === true) {
            setSort({
                name,
                desc: false,
            });
        } else if (sort === false) {
            setSort(null);
        } else {
            setSort({
                name,
                desc: true,
            });
        }
    };

    const contextRef = useRef(null);
    const thRef = useRef(null);
    const popupRef = useRef(null);



    return (
        <div className="facet" ref={thRef}>
            {notSortAndFilter ? (
                <div className={'facet-field__disabled'} ref={contextRef}>
                    {t(displayNameKey)}
                </div>
            ) : (
                <>
                    {notWrapHeader ? (
                        <div className="facet-field" onClick={noSort ? null : handleSort} ref={contextRef}>
                            <span dangerouslySetInnerHTML={{ __html: t(displayNameKey) }} />
                        </div>
                    ) : (
                        <div onClick={noSort ? null : handleSort} ref={contextRef}>
                            <TextCropping width={width} indexColumn={index}>
                                <span dangerouslySetInnerHTML={{ __html: t(displayNameKey) }} />
                            </TextCropping>
                        </div>
                    )}
                    <div className="facet-actions">
                        <div
                            className={
                                value ? 'facet-actions__filter_active' : 'facet-actions__filter'
                            }
                        >
                            <Popup
                                trigger={
                                    <Button>
                                        <Icon name="filter" />
                                    </Button>
                                }
                                context={contextRef}
                                pinned
                                basic
                                ref={popupRef}
                                position={'bottom center'}
                                className="from-popup"
                                on="click"
                            >
                                <Control
                                    type={type}
                                    name={name}
                                    value={value}
                                    source={source}
                                    ref={popupRef}
                                    emptyValue={emptyValue}
                                    gridName={gridName}
                                    filters={filters}
                                    onChange={handleChangeFilter}
                                    t={t}
                                />
                            </Popup>
                        </div>

                        <div className="facet-actions__sort">
                            {sort === false ? <Icon name="sort amount up" /> : null}
                            {sort === true ? <Icon name="sort amount down" /> : null}
                        </div>
                    </div>
                </>
            )}
        </div>
    );
};

export default React.memo(FacetField);
